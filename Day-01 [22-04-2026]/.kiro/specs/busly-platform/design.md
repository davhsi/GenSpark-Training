# Design Document — Busly Platform

## Overview

Busly is a full-stack online bus ticket booking platform that connects passengers with bus operators through a managed marketplace. The system supports three distinct roles — Customer, Bus Operator, and Admin — each with dedicated workflows, access controls, and dashboards.

The platform handles the complete lifecycle: route discovery → seat selection → seat locking → payment → ticket generation → cancellation/refund processing. It is built as a classic three-tier web application: an Angular SPA communicates exclusively with a .NET Web API, which in turn owns all database access through EF Core.

**Technology decisions (locked):**

| Concern | Choice | Rationale |
|---|---|---|
| Frontend | Angular + Bootstrap / Angular Material | Component-driven SPA with strong TypeScript support |
| Backend | .NET 10 Web API (C#) | Mature ecosystem, strong typing, excellent EF Core integration |
| Database | PostgreSQL | JSONB for seat config, `pg_trgm` for fuzzy search, strong constraint support |
| ORM | EF Core (Npgsql 9.x) | Code-first migrations, LINQ queries, raw SQL escape hatch |
| Auth | JWT stored in HttpOnly cookie | Stateless, role claims embedded in token; cookie prevents JS access |
| PDF | QuestPDF | Fluent C# API, no external process |
| Email | MailKit + `System.Threading.Channels` | Non-blocking SMTP dispatch |
| Password | BCrypt.Net-Next | Industry-standard adaptive hashing |
| Fuzzy search | `pg_trgm` GIN indexes | Server-side trigram matching, no external search engine |

---

## Architecture

### High-Level System Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        Browser (Angular SPA)                    │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌───────────────┐  │
│  │ AuthMod  │  │SearchMod │  │BookingMod│  │Admin/OpMod    │  │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └───────┬───────┘  │
│       │              │              │                │           │
│  ┌────┴──────────────┴──────────────┴────────────────┴───────┐  │
│  │  CoreModule: AuthService · JwtInterceptor · Route Guards  │  │
│  └───────────────────────────┬───────────────────────────────┘  │
└──────────────────────────────┼──────────────────────────────────┘
                               │ HTTPS / JSON
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                    .NET 8 Web API (Busly.API)                   │
│                                                                 │
│  Controllers ──► Services ──► Repositories ──► AppDbContext    │
│                     │                                           │
│  BackgroundJobs ────┘  (SeatLockExpiryJob, EmailDispatchJob)   │
│  Helpers: JwtHelper · RefundCalculator · CouponGenerator       │
└───────────────────────────────┬─────────────────────────────────┘
                                │ EF Core / Npgsql
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    PostgreSQL Database                          │
│  18 tables · uuid-ossp · pg_trgm · GIN indexes                 │
└─────────────────────────────────────────────────────────────────┘
```

### Request Flow

1. Angular component calls an Angular service method.
2. Angular service issues an HTTP request; `JwtInterceptor` attaches the `Authorization: Bearer` header.
3. .NET controller receives the request, validates the JWT, and checks the role policy.
4. Controller delegates to a service class (business logic).
5. Service calls one or more repository methods (data access).
6. Repository executes EF Core queries against PostgreSQL.
7. Response travels back up the chain as a DTO.

### Background Jobs

Three `IHostedService` implementations run inside the API process:

- **PlatformCleanupJob** — `PeriodicTimer` every 60 s; expires seat locks (`seat_lock.is_active = false` where `expires_at < NOW()`) and handles payment timeouts (reverts `PAYMENT_PENDING` bookings older than 15 minutes back to `INITIATED`).
- **EmailDispatchJob** — drains `Channel<EmailMessage>`; calls MailKit SMTP; writes `notification` row.
- **SeatLockCleanupService** — `BackgroundService` every 5 min; delegates to `ISeatLockSecurityService.CleanupExpiredLocksAsync()` for security-layer cleanup.

---

## Components and Interfaces

### .NET Project Structure

```
Busly.API/
  Controllers/
    AuthController.cs
    AdminController.cs
    AdminSeatLockController.cs      ← Admin force-release seat lock
    OperatorController.cs
    BookingController.cs
    CancellationController.cs
    CouponController.cs
    SearchController.cs
    PnrController.cs                ← PNR lookup + CAPTCHA
  Models/                    ← EF Core entity classes (one per table)
    Admin.cs
    Customer.cs
    BusOperator.cs
    Route.cs
    BusLayout.cs
    Bus.cs
    Seat.cs
    BusStop.cs
    BusOperatingDay.cs              ← NEW: days of week bus operates
    Coupon.cs
    Booking.cs
    BookedSeat.cs
    SeatLock.cs
    Payment.cs
    Cancellation.cs
    Notification.cs
    AuditLog.cs
    TcVersion.cs
    PlatformConfig.cs
  DTOs/                      ← Request / response shapes
    Auth/
    Admin/
    Booking/
    Cancellation/
    Operator/
      OperatingDayDto.cs / UpdateOperatingDaysRequest.cs
    PNR/
      PnrLookupRequest.cs / PnrLookupResponse.cs
    Search/
      BulkSeatLockRequest.cs / BulkSeatLockResponse.cs
  Repositories/
    IAuthRepository.cs / AuthRepository.cs
    IRouteRepository.cs / RouteRepository.cs
    IBusRepository.cs / BusRepository.cs
    IBookingRepository.cs / BookingRepository.cs
    ISeatRepository.cs / SeatRepository.cs
    ICancellationRepository.cs / CancellationRepository.cs
    ICouponRepository.cs / CouponRepository.cs
    IAuditRepository.cs / AuditRepository.cs
    IPaymentRepository.cs / PaymentRepository.cs
    ITcRepository.cs / TcRepository.cs
  Services/
    AuthService.cs
    AdminService.cs
    OperatorService.cs
    BookingService.cs
    CancellationService.cs
    EmailService.cs                 ← IEmailService + MailKit impl
    PdfService.cs                   ← QuestPDF ticket generation
    SearchService.cs
    SeatLockService.cs
    SeatLockSecurityService.cs      ← NEW: rate limiting + fraud detection
    PnrService.cs                   ← NEW: PNR lookup logic
    CaptchaService.cs               ← NEW: CAPTCHA generation/validation
    DateValidationService.cs        ← NEW: journey date range validation
    ConfigService.cs
  BackgroundJobs/
    PlatformCleanupJob.cs           ← Seat lock expiry + payment timeouts (60s)
    EmailDispatchJob.cs
    SeatLockCleanupService.cs       ← NEW: security cleanup (5 min)
  Helpers/
    CouponGenerator.cs
    RefundCalculatorService.cs
    RetryHelper.cs
  Middleware/
    RateLimitingMiddleware.cs       ← 30 req/min per IP
  Data/
    AppDbContext.cs
  Program.cs
  appsettings.json
```

### Angular Project Structure

```
src/app/
  core/
    services/
      auth.service.ts          ← login, register, currentUser$ BehaviorSubject
      jwt.interceptor.ts       ← attaches Authorization header
    guards/
      customer.guard.ts
      operator.guard.ts
      admin.guard.ts
  shared/
    components/
      seat-map/                ← reusable seat grid component
      ticket-card/
      navbar/
      tc-modal/
    pipes/
    models/                    ← TypeScript interfaces matching DTOs
  features/
    auth/
      login/
      register/
    search/
      home/                    ← autocomplete search form
      results/                 ← bus result cards
    booking/
      seat-selection/          ← seat map + lock timer
      booking-summary/         ← passenger details + coupon + fare
      payment/                 ← dummy payment form
      confirmation/            ← PNR + download ticket
    my-bookings/
      booking-list/
      booking-detail/
    operator/
      dashboard/
      bus-list/
      bus-register/
      layout-builder/
      boarding-points/
    admin/
      routes/
      operator-queue/
      bus-queue/
      revenue/
  app-routing.module.ts
  app.module.ts
```

### Key Angular–API Contracts (DTOs)

All API responses use camelCase JSON. Key shapes:

**LoginResponse** (cookie-based — token stored in HttpOnly cookie, not returned in body)
```json
{ "role": "Customer", "email": "user@example.com", "userId": "uuid" }
```

**BusSearchResult**
```json
{
  "busId": "uuid",
  "busName": "string",
  "busNumber": "string",
  "operatorName": "string",
  "departureTime": "08:00",
  "boardingCity": "string",
  "droppingCity": "string",
  "basePrice": 450.00,
  "availableSeats": 12
}
```

**SeatMapResponse**
```json
{
  "layoutConfig": { "rows": 10, "cols": 4, "decks": ["lower","upper"], "seats": [...] },
  "seatStatuses": [
    { "seatId": "uuid", "seatNumber": 1, "status": "AVAILABLE", "passengerGender": null }
  ]
}
```

**BookingCreateRequest**
```json
{
  "busId": "uuid",
  "journeyDate": "2026-05-01",
  "seatIds": ["uuid1", "uuid2"],
  "passengers": [
    { "seatId": "uuid1", "name": "Alice", "age": 28, "gender": "Female" }
  ],
  "couponCode": "BUSLY-XXXX"
}
```

**BulkSeatLockRequest**
```json
{
  "seatIds": ["uuid1", "uuid2"],
  "busId": "uuid",
  "journeyDate": "2026-05-01"
}
```

**BulkSeatLockResponse**
```json
{
  "successfulLocks": [ { "lockId": "uuid", "seatId": "uuid", ... } ],
  "failedSeatIds": ["uuid3"],
  "allSuccessful": false
}
```

**PnrLookupRequest**
```json
{
  "pnr": "ABCD1234",
  "captchaToken": "sessionId:CAPTCHATEXT",
  "captchaInput": "CAPTCHATEXT"
}
```

**PnrLookupResponse**
```json
{
  "pnr": "ABCD1234",
  "status": "CONFIRMED",
  "customerName": "Private Information",
  "customerEmail": "Private Information",
  "journeyDate": "2026-05-01",
  "fromCity": "Chennai",
  "toCity": "Bangalore",
  "busNumber": "TN01AB1234",
  "departureTime": "08:00",
  "arrivalTime": "14:00",
  "seatNumbers": ["5", "6"],
  "totalAmount": 950.00,
  "bookedAt": "2026-04-20T10:00:00Z",
  "canCancel": true,
  "cancellationReason": null,
  "refundAmount": null,
  "refundStatus": null
}
```

---

## Data Models

### Entity Relationship Summary

The 19 tables map to the following entity groups:

```
Identity:     admin · customer · bus_operator
Config:       platform_config · tc_version · route
Fleet:        bus_layout · bus · seat · bus_stop · bus_operating_days
Transaction:  booking · booked_seat · seat_lock · payment
Post-booking: cancellation · coupon · notification · audit_log
```

### All 19 Entities (from schema.sql)

#### 1. `admin`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | `uuid_generate_v4()` |
| username | TEXT | |
| password_hash | TEXT | BCrypt |
| email | TEXT UNIQUE | |

#### 2. `customer`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| username | TEXT | |
| password_hash | TEXT | BCrypt |
| email | TEXT UNIQUE | |
| name | TEXT | nullable, filled on first booking |
| age | INT | nullable |
| gender | TEXT | nullable |
| tc_accepted | BOOLEAN | default false |
| tc_version | TEXT | version string of accepted T&C |
| tc_accepted_at | TIMESTAMP | |
| created_at | TIMESTAMP | |

#### 3. `bus_operator`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| company_name | TEXT | |
| email | TEXT UNIQUE | |
| password_hash | TEXT | BCrypt |
| phone | TEXT | |
| status | TEXT | CHECK: PENDING/APPROVED/DISABLED/REJECTED |
| tc_accepted | BOOLEAN | |
| tc_version | TEXT | |
| tc_accepted_at | TIMESTAMP | |
| approved_by_admin | UUID FK → admin | |
| approved_at | TIMESTAMP | |
| created_at | TIMESTAMP | |

#### 4. `route`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| source_city | TEXT | |
| destination_city | TEXT | |
| is_active | BOOLEAN | default true |
| created_by_admin | UUID FK → admin | |
| UNIQUE | (source_city, destination_city) | |

#### 5. `bus_layout`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| layout_name | TEXT | |
| total_seats | INT | |
| seat_config | JSONB | see seat_config format below |

#### 6. `bus`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| operator_id | UUID FK → bus_operator | |
| route_id | UUID FK → route | |
| bus_number | TEXT | number plate |
| bus_name | TEXT | |
| owner_name | TEXT | |
| status | TEXT | CHECK: PENDING/ACTIVE/DISABLED/REMOVED |
| base_price | NUMERIC(10,2) | |
| layout_id | UUID FK → bus_layout | |
| approved_by_admin | UUID FK → admin | |
| approved_at | TIMESTAMP | |
| created_at | TIMESTAMP | |

#### 7. `seat`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| layout_id | UUID FK → bus_layout | |
| seat_number | INT | |
| seat_type | TEXT | window / aisle |
| deck | TEXT | lower / upper |

#### 8. `bus_stop`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| bus_id | UUID FK → bus | |
| type | TEXT | CHECK: BOARDING / DROPPING |
| city | TEXT | |
| address | TEXT | |
| scheduled_time | TIME | |

#### 9. `coupon`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| code | TEXT UNIQUE | |
| discount_value | NUMERIC(10,2) | |
| discount_type | TEXT | CHECK: flat / percent |
| issued_to_customer | UUID FK → customer | |
| cancellation_id | UUID FK → cancellation | |
| is_used | BOOLEAN | default false |
| expires_at | TIMESTAMP | NOW() + 30 days on creation |

#### 10. `booking`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| customer_id | UUID FK → customer | |
| bus_id | UUID FK → bus | |
| journey_date | DATE | |
| base_fare | NUMERIC(10,2) | base_price × seat_count |
| convenience_fee | NUMERIC(10,2) | from platform_config |
| total_amount | NUMERIC(10,2) | base_fare + fee − coupon |
| status | TEXT | CHECK: INITIATED/PAYMENT_PENDING/CONFIRMED/CANCELLED/REFUNDED |
| booked_at | TIMESTAMP | |
| coupon_id | UUID FK → coupon | nullable |

#### 11. `booked_seat`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| booking_id | UUID FK → booking ON DELETE CASCADE | |
| seat_id | UUID FK → seat | |
| bus_id | UUID FK → bus | denormalized for index |
| journey_date | DATE | denormalized for index |
| passenger_name | TEXT | |
| passenger_age | INT | |
| passenger_gender | TEXT | |
| UNIQUE | (seat_id, bus_id, journey_date) | prevents double booking |

#### 12. `seat_lock`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| seat_id | UUID FK → seat | |
| customer_id | UUID FK → customer | |
| bus_id | UUID FK → bus | |
| journey_date | DATE | |
| locked_at | TIMESTAMP | |
| expires_at | TIMESTAMP | NOW() + 10 min |
| is_active | BOOLEAN | default true |

#### 13. `payment`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| booking_id | UUID FK → booking | 1:1 |
| amount | NUMERIC(10,2) | |
| status | TEXT | CHECK: PENDING/SUCCESS/FAILED/REFUNDED |
| transaction_ref | TEXT | dummy gateway ref |
| paid_at | TIMESTAMP | |

#### 14. `cancellation`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| booking_id | UUID FK → booking | |
| cancelled_by | TEXT | CHECK: customer / operator |
| cancelled_at | TIMESTAMP | |
| refund_amount | NUMERIC(10,2) | |
| refund_status | TEXT | CHECK: PENDING/PROCESSED/FAILED |

#### 15. `notification`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| customer_id | UUID FK → customer | nullable |
| operator_id | UUID FK → bus_operator | nullable |
| type | TEXT | e.g. BOOKING_CONFIRMED, CANCELLATION |
| message | TEXT | |
| status | TEXT | SENT / FAILED |
| sent_at | TIMESTAMP | |
| CHECK | exactly one of customer_id / operator_id is non-null | |

#### 16. `audit_log`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| actor_id | UUID | |
| actor_role | TEXT | CHECK: admin / customer / operator |
| action | TEXT | e.g. APPROVE_OPERATOR |
| entity_type | TEXT | e.g. bus_operator |
| entity_id | UUID | |
| metadata | JSONB | optional extra context |
| performed_at | TIMESTAMP | |

#### 18. `tc_version`
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| version | TEXT UNIQUE | e.g. "v1.2" |
| content | TEXT | full T&C text |
| published_at | TIMESTAMP | |
| published_by_admin | UUID FK → admin | |
| is_active | BOOLEAN | only one active at a time |

#### 19. `platform_config`
| Column | Type | Notes |
|---|---|---|
| key | TEXT PK | e.g. "convenience_fee_type" |
| value | TEXT | e.g. "flat" or "percent" |

#### 20. `bus_operating_days` *(new)*
| Column | Type | Notes |
|---|---|---|
| id | UUID PK | |
| bus_id | UUID FK → bus | |
| day_of_week | INT | 1=Monday … 7=Sunday |
| is_active | BOOLEAN | |
| created_at | TIMESTAMP | |
| updated_at | TIMESTAMP | |

### seat_config JSONB Format

This format is the contract between the .NET layout parser and the Angular seat map renderer. It must not change after Sprint 3.

```json
{
  "rows": 10,
  "cols": 4,
  "decks": ["lower", "upper"],
  "seats": [
    { "seat_number": 1, "row": 1, "col": 1, "type": "window", "deck": "lower" },
    { "seat_number": 2, "row": 1, "col": 2, "type": "aisle",  "deck": "lower" },
    { "seat_number": 3, "row": 1, "col": 3, "type": "aisle",  "deck": "lower" },
    { "seat_number": 4, "row": 1, "col": 4, "type": "window", "deck": "lower" }
  ]
}
```

The Angular seat map component reads `rows`, `cols`, and `decks` to build the grid skeleton, then places each seat object at `[row][col]` within the correct deck panel.

### Critical Database Indexes

```sql
-- Fast seat lock lookup (used on every seat map load)
CREATE INDEX idx_seat_lock_lookup
  ON seat_lock (seat_id, bus_id, journey_date, is_active);

-- Prevent double booking at DB level
CREATE UNIQUE INDEX unique_seat_per_trip
  ON booked_seat (seat_id, bus_id, journey_date);

-- Booking history queries
CREATE INDEX idx_booking_customer ON booking (customer_id);
CREATE INDEX idx_booking_bus      ON booking (bus_id);

-- Fuzzy city search
CREATE INDEX idx_route_source_trgm
  ON route USING gin (source_city gin_trgm_ops);
CREATE INDEX idx_route_destination_trgm
  ON route USING gin (destination_city gin_trgm_ops);
```

---

## Key Architectural Decisions

### 1. Booking State Machine

Bookings transition through exactly five states. No other transition is permitted; the service layer enforces this before any DB write.

```
INITIATED ──► PAYMENT_PENDING ──► CONFIRMED ──► CANCELLED ──► REFUNDED
                    │
                    └──► INITIATED  (on payment failure — lock released)
```

State transition rules:
- `INITIATED → PAYMENT_PENDING`: customer calls `/bookings/{id}/pay`
- `PAYMENT_PENDING → CONFIRMED`: dummy gateway returns success
- `PAYMENT_PENDING → INITIATED`: dummy gateway returns failure (locks released)
- `CONFIRMED → CANCELLED`: customer cancellation or operator cascade
- `CANCELLED → REFUNDED`: internal refund-complete call after dummy refund processed

### 2. Seat Availability — Single Query Rule

Seat availability is always calculated with one SQL JOIN. Never use multiple round trips.

```sql
SELECT
  s.id,
  s.seat_number,
  s.seat_type,
  s.deck,
  CASE
    WHEN bs.id IS NOT NULL THEN 'BOOKED'
    WHEN sl.id IS NOT NULL THEN 'LOCKED'
    ELSE 'AVAILABLE'
  END AS status,
  bs.passenger_gender
FROM seat s
LEFT JOIN booked_seat bs
  ON bs.seat_id = s.id
  AND bs.bus_id = @busId
  AND bs.journey_date = @date
LEFT JOIN seat_lock sl
  ON sl.seat_id = s.id
  AND sl.bus_id = @busId
  AND sl.journey_date = @date
  AND sl.is_active = true
WHERE s.layout_id = (SELECT layout_id FROM bus WHERE id = @busId)
```

Rationale: multiple sequential queries create a TOCTOU window where a seat could appear available in query 1 but be locked by the time query 2 runs.

### 3. Seat Lock Expiry — Background Job

Locks are expired by `SeatLockExpiryJob` (an `IHostedService` using `PeriodicTimer`, 60-second interval). Expiry is never done inline on the seat map API endpoint — that would create race conditions between the expiry check and the lock insert.

### 4. Idempotency on Booking Creation

Before inserting a new booking, `BookingService` checks: does a `CONFIRMED` or `PAYMENT_PENDING` booking already exist for this `customer_id` + `seat_id` + `journey_date`? If yes, return the existing booking. This prevents duplicate charges on network retries.

### 5. Email Queue — Never Block HTTP

`IEmailService` writes to a `Channel<EmailMessage>`. The HTTP response returns immediately. `EmailDispatchJob` (another `IHostedService`) drains the channel and calls MailKit SMTP. On failure, it updates `notification.status = 'FAILED'` for retry visibility.

### 6. T&C Version Check on Booking

On every booking attempt (not on every login), the backend checks `customer.tc_version == latest active tc_version.version`. If not, it returns `403 TC_REACCEPTANCE_REQUIRED`. The Angular `JwtInterceptor` (or a dedicated HTTP response interceptor) catches this code and shows the T&C modal before retrying.

### 7. Logical Deletes Only

`booking`, `payment`, and `cancellation` rows are never physically deleted. The `bus` table uses `status = 'REMOVED'` instead of `DELETE`. This preserves the full audit trail.

### 8. Operator Cancellation Cascade

When a bus is disabled or removed, the entire cascade runs inside a single database transaction:

```
For each CONFIRMED future booking on this bus:
  1. INSERT cancellation (cancelled_by='operator', refund_amount=total_amount, refund_status='PENDING')
  2. UPDATE booking SET status='CANCELLED'
  3. INSERT coupon (unique code, flat discount, expires NOW()+30d, issued_to customer)
  4. Enqueue email (cancellation notice + coupon code)
```

### 9. Cancellation Refund Rules

| Hours before departure | Triggered by | Refund base | Convenience fee |
|---|---|---|---|
| > 24 h | Customer | 85% of `base_fare` | Not refunded |
| 12–24 h | Customer | 50% of `base_fare` | Not refunded |
| < 12 h | Customer | 0% | Not refunded |
| Any | Operator (bus disabled/removed) | 100% of `total_amount` | Refunded |

`RefundCalculatorService` encapsulates this logic. It takes `(journeyDate, departureTime, cancelledBy, baseFare, totalAmount)` and returns `refundAmount`.

### 10. JWT Stored in HttpOnly Cookie

The JWT is stored in an HttpOnly cookie (`busly_token`) rather than returned in the response body. This prevents JavaScript access to the token. The `JwtBearerEvents.OnMessageReceived` handler reads the cookie on every request. The login response body returns only `role`, `email`, and `userId`. CORS is configured with `AllowCredentials()` so the Angular client can send cookies cross-origin.

### 11. Rate Limiting Middleware

`RateLimitingMiddleware` enforces 30 requests per minute per IP address. It skips `/health` and `/admin` paths. IP detection supports `X-Forwarded-For` and `X-Real-IP` headers for reverse-proxy deployments.

### 12. CAPTCHA for PNR Lookup

PNR lookup is public (unauthenticated) and could be scraped. A simple server-side CAPTCHA protects it: `GET /captcha` returns a `sessionId` and a 6-character alphanumeric challenge. The client submits both the `sessionId:captchaText` token and the user's input with the PNR lookup request. Sessions expire after 5 minutes and allow at most 3 attempts. Sessions are stored in a static in-memory dictionary (suitable for single-instance deployments).

### 13. Seat Lock Security Layer

`SeatLockSecurityService` adds a fraud-detection layer on top of the basic seat lock logic:
- 5 lock attempts per minute per IP (via `IMemoryCache`)
- 10 lock attempts per customer per hour
- Max 4 active locks per customer at any time
- Suspicious pattern detection: >10 attempts from same IP in 5 min, >3 customer accounts from same IP in 24 h, customer locking seats on >5 different buses in 1 h

`SeatLockCleanupService` (a `BackgroundService`) runs every 5 minutes and calls `ISeatLockSecurityService.CleanupExpiredLocksAsync()`.

### 14. Journey Date Validation

`DateValidationService` enforces two validation levels:
- **Seat lock**: journey date must be today or later, within 90 days, and not after 20:00 UTC for same-day requests.
- **Booking creation**: journey date must be tomorrow or later (no same-day bookings), within 90 days.

### 15. Layered Build Order

Every feature must be built in this order. Never build a component before its API endpoint exists.

```
DB Migration → EF Core Entity → Repository → Service → Controller
    → Angular Service → Angular Component
```

---

## API Design

All endpoints are grouped by controller and sprint.

### Sprint 1 — AuthController

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/auth/register/customer` | None | Hash password, create customer, `tc_accepted=false` |
| POST | `/auth/register/operator` | None | Hash password, create operator, `status='PENDING'`, `tc_accepted=false` |
| POST | `/auth/login` | None | Detect role from DB, return JWT in HttpOnly cookie (`busly_token`); body returns `role`, `email`, `userId` |
| POST | `/auth/logout` | None | Clear `busly_token` cookie |
| POST | `/auth/accept-tc` | Any role | Update `tc_version` + `tc_accepted_at` for caller |
| GET | `/auth/tc-status` | Any role | Return whether caller has accepted the current T&C version |
| GET | `/auth/me` | Any role | Return current user profile from JWT claims |

### Sprint 2 — AdminController (Routes & Approvals)

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/admin/routes` | Admin | Create city-pair route; write audit log |
| GET | `/routes` | None | List all active routes (public) |
| PATCH | `/admin/routes/{id}/toggle` | Admin | Toggle `is_active`; write audit log |
| GET | `/admin/operators/pending` | Admin | List operators with `status='PENDING'` |
| GET | `/admin/operators` | Admin | List all operators |
| PATCH | `/admin/operators/{id}/approve` | Admin | Set `status='APPROVED'`; write audit log |
| PATCH | `/admin/operators/{id}/reject` | Admin | Set `status='REJECTED'`; write audit log |
| PATCH | `/admin/operators/{id}/toggle` | Admin | Toggle APPROVED ↔ DISABLED |
| GET | `/admin/buses/pending` | Admin | List buses with `status='PENDING'` |
| GET | `/admin/buses` | Admin | List all buses |
| GET | `/admin/operators/{operatorId}/buses` | Admin | List buses for a specific operator |
| PATCH | `/admin/buses/{id}/approve` | Admin | Set `status='ACTIVE'`; write audit log |
| PATCH | `/admin/buses/{id}/reject` | Admin | Set `status='PENDING'` with reason; write audit log |
| PATCH | `/admin/buses/{id}/toggle` | Admin | Toggle bus active/disabled status |
| GET | `/admin/revenue` | Admin | Sum of `convenience_fee` across CONFIRMED bookings, grouped by month |
| GET | `/admin/revenue/by-operator` | Admin | Per-operator: booking count + total fare + total convenience fee |
| GET | `/admin/tc` | Admin | List all T&C versions |
| GET | `/tc/current` | None | Get current active T&C (public) |
| POST | `/admin/tc` | Admin | Publish new T&C version |
| GET | `/admin/audit-logs` | Admin | Get audit log entries |
| DELETE | `/admin/seats/lock/{id}` | Admin | Force-release a seat lock |

### Sprint 3 — OperatorController

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/operator/layouts` | Operator | Create bus layout + generate seat rows |
| GET | `/operator/layouts` | Operator | List own layouts |
| DELETE | `/operator/layouts/{id}` | Operator | Delete layout (if not in use) |
| POST | `/operator/buses` | Operator | Register bus, `status='PENDING'` |
| GET | `/operator/buses` | Operator | List all buses for this operator |
| GET | `/operator/buses/{id}` | Operator | Get specific bus details |
| POST | `/operator/buses/{id}/boarding-points` | Operator | Add boarding stop |
| POST | `/operator/buses/{id}/dropping-points` | Operator | Add dropping stop |
| DELETE | `/operator/buses/stops/{id}` | Operator | Remove a bus stop |
| PATCH | `/operator/buses/{id}/price` | Operator | Update `base_price` |
| PATCH | `/operator/buses/{id}/staff` | Operator | Update driver/conductor info |
| PATCH | `/operator/buses/{id}/disable` | Operator | Set `status='DISABLED'`; trigger cascade |
| DELETE | `/operator/buses/{id}` | Operator | Set `status='REMOVED'`; trigger cascade |
| PUT | `/operator/buses/{busId}/operating-days` | Operator | Set days of week bus operates |
| GET | `/operator/bookings` | Operator | All bookings across operator's buses |
| GET | `/operator/profile` | Operator | Get operator profile |

### Sprint 4 — SearchController

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/buses/search` | None | `?from=&to=&date=` — active buses with available seat count |
| GET | `/buses/{id}/seats` | None | `?date=` — full seat layout with per-seat status and gender |
| GET | `/buses/{id}` | None | Get bus details |
| GET | `/cities/autocomplete` | None | `?q=` — fuzzy city search via `pg_trgm` ILIKE |
| POST | `/seats/lock` | Customer | Insert seat lock, `expires_at = NOW() + 10 min` |
| POST | `/seats/lock/bulk` | Customer | Lock up to 4 seats in one request |
| PUT | `/seats/lock/{id}/extend` | Customer | Extend lock expiry |
| GET | `/seats/lock/my-locks` | Customer | Get caller's active seat locks |
| DELETE | `/seats/lock/{id}` | Customer | Release lock early |
| DELETE | `/seats/lock/by-seat/{seatId}` | Customer | Release lock by seat ID |

### Sprint 5 — BookingController

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/bookings` | Customer | Create booking `INITIATED`; idempotency check; create booked_seat rows |
| POST | `/bookings/{id}/pay` | Customer | Move to `PAYMENT_PENDING`; call dummy payment; on success → `CONFIRMED` |
| GET | `/bookings/mine` | Customer | Customer's full booking history |
| GET | `/bookings/{id}/ticket` | Customer | Stream PDF ticket (owner only) |

### Sprint 6 — CancellationController, CouponController, Admin Revenue

**CancellationController**

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/bookings/{id}/cancel` | Customer | Apply time-based refund rule; insert cancellation; set `CANCELLED`; enqueue email |
| POST | `/bookings/{id}/refund-complete` | Admin | Set `status='REFUNDED'` after dummy refund processed |

**CouponController**

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/bookings/apply-coupon` | Customer | Validate coupon (not expired, not used, issued to caller); return discount |

### Sprint 7 — PNR Lookup, Security & Operating Days *(new)*

**PnrController**

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/captcha` | None | Generate CAPTCHA session; return `sessionId` and `captchaText` |
| POST | `/pnr/lookup` | None | Validate CAPTCHA; look up booking by 8-char PNR; return privacy-safe details |

**AdminSeatLockController**

| Method | Route | Auth | Description |
|---|---|---|---|
| DELETE | `/admin/seats/lock/{id}` | Admin | Force-release any active seat lock |

---

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system — essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*


### Property Reflection

Before writing the final properties, I'll consolidate redundant ones:

- 1.1 and 1.2 (registration invariants) can be combined: for any registration of any role, the record must have a hashed password and the correct initial status/tc_accepted values.
- 2.1, 2.2, 2.3 (RBAC) — 2.2 and 2.3 are instances of 2.1; combine into one RBAC property.
- 11.1 and 11.5 (fare calculation) — both test arithmetic on booking creation; combine into one fare calculation property.
- 20.3 and 20.4 (convenience fee) — both test the fee formula; combine into one convenience fee property.
- 15.1 and 15.2 (refund calculation) — both test refund arithmetic; combine into one refund calculation property.
- 14.1 and 14.5 (cascade) — combine into one cascade atomicity property.

---

### Property 1: Registration Produces Correct Initial State

*For any* valid registration request (Customer or Operator), the created record SHALL have a password hash that does not equal the plaintext password, `tc_accepted = false`, and — for Operators — `status = 'PENDING'`.

**Validates: Requirements 1.1, 1.2**

---

### Property 2: Login JWT Contains Required Claims

*For any* registered user with valid credentials, the returned JWT SHALL contain the claims `sub` (matching the user's ID), `role` (matching the user's role), and `email` (matching the user's registered email).

**Validates: Requirements 1.3**

---

### Property 3: Invalid Credentials Always Return 401

*For any* login request where the email does not exist or the password does not match the stored hash, the API SHALL return a 401 Unauthorized response.

**Validates: Requirements 1.4**

---

### Property 4: Role-Based Access Control Enforcement

*For any* protected API endpoint and any request that either lacks a valid JWT or carries a JWT with an insufficient role, the API SHALL return 401 (no token) or 403 (wrong role) respectively — never a 2xx response.

**Validates: Requirements 2.1, 2.2, 2.3**

---

### Property 5: T&C Acceptance Round Trip

*For any* user (Customer or Operator) who calls the T&C acceptance endpoint, the user's stored `tc_version` SHALL equal the currently active `TC_Version.version` and `tc_accepted_at` SHALL be set to a non-null timestamp.

**Validates: Requirements 3.1**

---

### Property 6: Stale T&C Blocks Booking

*For any* Customer whose stored `tc_version` does not match the latest active `TC_Version`, any booking creation attempt SHALL return a 403 response with error code `TC_REACCEPTANCE_REQUIRED`.

**Validates: Requirements 3.2**

---

### Property 7: At Most One Active TC Version

*For any* sequence of TC version publications, after each publication exactly one `TC_Version` record SHALL have `is_active = true` and all others SHALL have `is_active = false`.

**Validates: Requirements 3.5**

---

### Property 8: Route Creation Produces Active Route

*For any* Admin route creation request with a unique city pair, the created Route SHALL have `is_active = true` and `created_by_admin` set to the requesting Admin's ID.

**Validates: Requirements 4.1**

---

### Property 9: Duplicate Route Returns 409

*For any* city pair that already exists as a Route, a second creation request with the same pair SHALL return a 409 Conflict response without creating a duplicate record.

**Validates: Requirements 4.2**

---

### Property 10: Route Toggle Is a Round Trip

*For any* Route, toggling `is_active` twice SHALL return the Route to its original `is_active` value.

**Validates: Requirements 4.3**

---

### Property 11: Seat Count Matches seat_config

*For any* valid `seat_config` JSONB payload submitted to the layout creation endpoint, the number of `Seat` records created in the database SHALL equal the number of objects in the `seats` array of the payload.

**Validates: Requirements 6.1**

---

### Property 12: Search Returns Only Active Buses on Matching Route

*For any* search query `(from, to, date)`, all buses returned SHALL have `status = 'ACTIVE'` and SHALL be bound to a Route where `source_city = from` and `destination_city = to`.

**Validates: Requirements 8.1, 8.5, 7.4**

---

### Property 13: Available Seat Count Is Arithmetically Correct

*For any* bus and journey date, the `availableSeats` value returned by the search endpoint SHALL equal `total_seats − (confirmed booked seats for that date) − (active locked seats for that date)`.

**Validates: Requirements 8.2**

---

### Property 14: Seat Lock Created with Correct Expiry

*For any* valid seat lock request, the created `Seat_Lock` record SHALL have `is_active = true` and `expires_at` within a 10-minute window of `locked_at`.

**Validates: Requirements 10.1**

---

### Property 15: Conflicting Seat Lock Returns 409

*For any* seat that already has an active `Seat_Lock` or a `Booked_Seat` for the same bus and journey date, a new lock request for that seat SHALL return a 409 Conflict response.

**Validates: Requirements 10.2**

---

### Property 16: Expired Locks Are Deactivated by Background Job

*For any* `Seat_Lock` record where `expires_at < NOW()` and `is_active = true`, after the `SeatLockExpiryJob` runs, the record SHALL have `is_active = false`.

**Validates: Requirements 10.4**

---

### Property 17: Booking Fare Calculation Is Correct

*For any* booking creation request with `seat_count` seats on a bus with `base_price`, the resulting Booking SHALL satisfy:
- `base_fare = base_price × seat_count`
- `convenience_fee` equals the configured flat amount or `base_fare × (percent / 100)`
- `total_amount = base_fare + convenience_fee − coupon_discount`
- `status = 'INITIATED'`

**Validates: Requirements 11.1, 11.2, 11.5, 20.3, 20.4**

---

### Property 18: Booking Creation Is Idempotent

*For any* booking creation request, submitting the same request (same `customer_id`, `seat_id`, `journey_date`) multiple times when a `CONFIRMED` or `PAYMENT_PENDING` booking already exists SHALL return the existing booking record without creating a new one.

**Validates: Requirements 11.3**

---

### Property 19: Coupon Validation Rejects Invalid Coupons

*For any* coupon that is expired, already used, or issued to a different customer, applying it during booking SHALL be rejected with an appropriate error response.

**Validates: Requirements 11.4**

---

### Property 20: Booking State Machine Enforces Valid Transitions Only

*For any* booking, only the defined state transitions (`INITIATED → PAYMENT_PENDING → CONFIRMED → CANCELLED → REFUNDED`, plus `PAYMENT_PENDING → INITIATED` on failure) SHALL be accepted. Any attempt to perform an undefined transition SHALL be rejected.

**Validates: Requirements 12.4**

---

### Property 21: Payment Failure Rolls Back Correctly

*For any* booking in `PAYMENT_PENDING` state where the payment gateway returns failure, the Booking SHALL revert to `INITIATED`, the Payment record SHALL have `status = 'FAILED'`, and all associated `Seat_Lock` records SHALL be released (`is_active = false`).

**Validates: Requirements 12.3**

---

### Property 22: Operator Cancellation Cascade Is Atomic and Complete

*For any* bus with N future `CONFIRMED` bookings, when the bus is set to `DISABLED` or `REMOVED`, the cascade SHALL atomically create exactly N `Cancellation` records (`cancelled_by = 'operator'`, `refund_amount = total_amount`) and exactly N `Coupon` records, and set all N bookings to `CANCELLED`. If any step fails, no partial state SHALL persist.

**Validates: Requirements 14.1, 14.2, 14.3, 14.5**

---

### Property 23: Customer Cancellation Refund Follows Tiered Rules

*For any* customer cancellation, the `refund_amount` in the `Cancellation` record SHALL equal:
- `0.85 × base_fare` if hours until departure > 24
- `0.50 × base_fare` if 12 ≤ hours until departure ≤ 24
- `0` if hours until departure < 12

The `convenience_fee` SHALL NOT be included in the refund amount.

**Validates: Requirements 15.1, 15.2**

---

### Property 24: Monthly Revenue Aggregation Is Correct

*For any* set of `CONFIRMED` bookings, the monthly revenue endpoint SHALL return sums of `convenience_fee` that equal the arithmetic sum of `convenience_fee` for all `CONFIRMED` bookings in each calendar month.

**Validates: Requirements 16.1**

---

### Property 25: Logical Deletion Preserves Records

*For any* booking cancellation or status change, the original `booking`, `payment`, and `cancellation` records SHALL remain in the database with updated `status` fields — no rows SHALL be physically deleted.

**Validates: Requirements 19.1**

---

### Property 26: Audit Log Is Written for Every Critical Action

*For any* critical action (operator approval/rejection, bus approval/rejection, route creation/toggle, booking creation, payment update, cancellation), an `Audit_Log` entry SHALL exist with the correct `actor_id`, `actor_role`, `action`, `entity_type`, `entity_id`, and `performed_at`.

**Validates: Requirements 18.1, 18.2, 4.5, 5.2**

---

## Error Handling

### HTTP Status Code Conventions

| Scenario | Status Code |
|---|---|
| Unauthenticated request to protected endpoint | 401 Unauthorized |
| Authenticated but insufficient role | 403 Forbidden |
| T&C version mismatch on booking | 403 with body `{ "code": "TC_REACCEPTANCE_REQUIRED" }` |
| Resource not found | 404 Not Found |
| Duplicate resource (route, seat lock conflict) | 409 Conflict |
| Validation error (malformed request body) | 400 Bad Request |
| Successful creation | 201 Created |
| Successful update | 200 OK |
| Successful no-content operation | 204 No Content |

### Failure Scenarios and Handling

**Payment failure**: The service sets `payment.status = 'FAILED'`, releases all `seat_lock` records for the booking, and reverts `booking.status` to `'INITIATED'`. The customer can retry payment.

**SMTP failure**: The `EmailDispatchJob` catches the exception, sets `notification.status = 'FAILED'`, and logs the error. The HTTP response has already returned — the customer is not blocked. Failed notifications are visible in the `notification` table for manual retry.

**Seat lock conflict**: If two customers attempt to lock the same seat simultaneously, the second request receives a 409. The unique index on `booked_seat (seat_id, bus_id, journey_date)` provides a final DB-level guard against double booking.

**Operator cascade failure**: The entire cascade runs in a single `DbContext` transaction. If any step throws, `SaveChanges` is not called and no partial state is written.

**Invalid state transition**: `BookingService` checks the current status before any transition. If the transition is not in the allowed set, it throws a domain exception that the controller maps to a 400 or 409 response.

**Expired seat lock on booking**: If a customer's seat lock has expired by the time they submit the booking, the booking creation fails with a 409 (no active lock for seat). The Angular client's countdown timer should prevent this in normal flow.

---

## Testing Strategy

### Dual Testing Approach

Both unit/example-based tests and property-based tests are used. They are complementary:

- **Unit tests** cover specific examples, integration points, and error conditions.
- **Property tests** verify universal invariants across a wide input space.

### Property-Based Testing

**Library**: [FsCheck](https://fscheck.github.io/FsCheck/) for .NET (integrates with xUnit). For Angular, [fast-check](https://fast-check.dev/) is used.

Each property test runs a minimum of **100 iterations**.

Tag format for each property test:
```
// Feature: busly-platform, Property {N}: {property_text}
```

**Properties to implement as PBT (mapped to design properties above):**

| Property | Test focus | Library |
|---|---|---|
| P1: Registration initial state | BCrypt hash check, status/tc_accepted | FsCheck (.NET) |
| P2: JWT claims completeness | Decode JWT, verify claims | FsCheck (.NET) |
| P3: Invalid credentials → 401 | Random wrong passwords/emails | FsCheck (.NET) |
| P4: RBAC enforcement | Cross-role endpoint matrix | FsCheck (.NET) |
| P5: T&C acceptance round trip | Accept → query → verify version | FsCheck (.NET) |
| P6: Stale T&C blocks booking | Outdated version → 403 | FsCheck (.NET) |
| P7: At most one active TC version | Publish N versions → count active | FsCheck (.NET) |
| P10: Route toggle round trip | Toggle twice → original value | FsCheck (.NET) |
| P11: Seat count matches config | Random seat_config → count seats | FsCheck (.NET) |
| P12: Search filter correctness | Mixed-status buses → only ACTIVE returned | FsCheck (.NET) |
| P13: Available seat count arithmetic | Known booked/locked → verify count | FsCheck (.NET) |
| P14: Seat lock expiry window | Lock → verify expires_at ≈ NOW()+10m | FsCheck (.NET) |
| P15: Conflicting lock → 409 | Lock seat → lock again → 409 | FsCheck (.NET) |
| P16: Background job expires locks | Past-expiry locks → is_active=false | FsCheck (.NET) |
| P17: Fare calculation | Random prices/counts → verify formula | FsCheck (.NET) |
| P18: Booking idempotency | Duplicate requests → single record | FsCheck (.NET) |
| P20: State machine transitions | Invalid transitions → rejected | FsCheck (.NET) |
| P21: Payment failure rollback | Fail payment → verify cleanup | FsCheck (.NET) |
| P22: Cascade atomicity | N bookings → N cancellations + N coupons | FsCheck (.NET) |
| P23: Refund tiered rules | Random departure times → verify % | FsCheck (.NET) |
| P24: Revenue aggregation | Random bookings → verify monthly sums | FsCheck (.NET) |
| P25: Logical deletion | Cancel → record still exists | FsCheck (.NET) |
| P26: Audit log completeness | Critical actions → audit entries | FsCheck (.NET) |

### Unit / Example-Based Tests

- **AuthController**: login with each role, register with duplicate email (409), register with missing fields (400).
- **BookingController**: booking with expired lock (409), booking with used coupon (400), ticket download by non-owner (403).
- **CancellationController**: cancel a non-CONFIRMED booking (400), cancel within 12h (0% refund example).
- **AdminController**: approve already-approved operator (idempotent), reject with audit log.
- **SeatMapComponent** (Angular): color coding for each seat status (AVAILABLE/LOCKED/BOOKED/SELECTED).
- **TcModalComponent** (Angular): modal shown on TC_REACCEPTANCE_REQUIRED response.

### Integration Tests

- Full booking happy path: register → login → search → lock seat → create booking → pay → confirm → download ticket.
- Operator cascade: register operator → register bus → approve → create bookings → disable bus → verify cancellations and coupons.
- Revenue endpoint: create confirmed bookings across months → verify monthly sums.

### Smoke Tests

- All 18 tables exist with correct columns and constraints (`\d tablename` in psql).
- `uuid-ossp` and `pg_trgm` extensions are enabled.
- JWT secret, DB connection string, and SMTP credentials are read from environment variables (not hardcoded).
- JWT stored in Angular memory (not localStorage/sessionStorage) — verified by code review.
- Seat availability uses single SQL JOIN — verified by code review.
