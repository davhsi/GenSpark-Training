# Busly — Implementation Plan
**Platform:** Online Bus Ticket Booking  
**Stack:** Angular + Bootstrap/Material · .NET Web API (C#) · PostgreSQL  
**Roles:** Customer · Bus Operator · Admin  
**Total Tables:** 18  
**Status:** Phase 1 — Implementation

---

## Ground Rules (Read Before Every Sprint)

1. **Layer order is mandatory:** DB migration check → EF Core entity → Repository → Service → Controller → Angular Service → Angular Component. Never build a component before its API endpoint exists.
2. **Test every endpoint in Swagger** (`/swagger`) before touching Angular.
3. **No raw SQL in controllers.** All DB access goes through a repository layer.
4. **All status fields use CHECK-constrained string enums** — never write a raw string value, always use constants or enums in C#.
5. **Logical deletes only** on `booking`, `payment`, `cancellation` — never `DELETE` these rows.
6. **JWT claims must include:** `sub` (user id), `role` (`Admin` / `Operator` / `Customer`), `email`.
7. **Environment variables** for all secrets: DB connection string, JWT secret, SMTP credentials.

---

## Database — 18 Tables Reference

| # | Table | Purpose |
|---|-------|---------|
| 1 | `admin` | Platform superuser accounts |
| 2 | `customer` | Passenger accounts with T&C metadata |
| 3 | `bus_operator` | Operator accounts with approval status and T&C metadata |
| 4 | `route` | Admin-defined city-pair routes |
| 5 | `bus_layout` | Reusable seat layout templates with JSONB seat config |
| 6 | `bus` | Operator-registered buses bound to routes and layouts |
| 7 | `seat` | Individual seats derived from a bus_layout |
| 8 | `bus_stop` | Operator-defined boarding and dropping locations per bus per city (type: BOARDING/DROPPING) |
| 9 | `coupon` | Discount codes issued on operator-triggered cancellations |
| 10 | `booking` | Core booking record with state machine and fare breakdown |
| 11 | `booked_seat` | Per-seat passenger details linked to a booking |
| 12 | `seat_lock` | Temporary seat reservation during payment grace period |
| 13 | `payment` | Payment record linked 1:1 to a booking |
| 14 | `cancellation` | Cancellation record with refund amount and trigger actor |
| 15 | `notification` | SMTP dispatch log per customer |
| 16 | `audit_log` | Immutable log of all critical admin/system actions |
| 17 | `tc_version` | Published T&C versions with content; drives mandatory re-acceptance |

### Critical Indexes
```sql
-- Fast seat lock lookup (used on every seat map load)
CREATE INDEX idx_seat_lock_lookup ON seat_lock (seat_id, journey_date, is_active);

-- Prevent double booking at DB level
CREATE UNIQUE INDEX unique_seat_per_trip ON booked_seat (seat_id, journey_date);

-- Booking history queries
CREATE INDEX idx_booking_customer ON booking (customer_id);
CREATE INDEX idx_booking_bus ON booking (bus_id);
```

### Booking State Machine
```
INITIATED → PAYMENT_PENDING → CONFIRMED → CANCELLED → REFUNDED
```
State transitions must be atomic. A booking is CONFIRMED **only** after successful payment response.

### Status CHECK Constraints
| Table | Allowed Values |
|-------|---------------|
| `bus_operator.status` | `PENDING`, `APPROVED`, `DISABLED`, `REJECTED` |
| `bus.status` | `PENDING`, `ACTIVE`, `DISABLED`, `REMOVED` |
| `booking.status` | `INITIATED`, `PAYMENT_PENDING`, `CONFIRMED`, `CANCELLED`, `REFUNDED` |
| `payment.status` | `PENDING`, `SUCCESS`, `FAILED`, `REFUNDED` |
| `cancellation.cancelled_by` | `customer`, `operator` |
| `cancellation.refund_status` | `PENDING`, `PROCESSED`, `FAILED` |
| `coupon.discount_type` | `flat`, `percent` |

---

## Sprint 1 — Foundation: Auth & Role Setup
**Modules:** M-01  
**Depends on:** Nothing — build this first

### DB
- Run full SQL schema; verify all 18 tables, indexes, and CHECK constraints in psql using `\d tablename`
- Enable `uuid-ossp` and `pg_trgm` extensions

### .NET Setup
- Create Web API project
- Install packages:
  - `Npgsql.EntityFrameworkCore.PostgreSQL`
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `BCrypt.Net-Next`
  - `MailKit` (for SMTP in Sprint 5)
  - `QuestPDF` (for ticket PDF in Sprint 5)
- Create `AppDbContext` with all 18 entity models
- Register JWT middleware in `Program.cs` with 3 role policies: `Admin`, `Operator`, `Customer`
- Create `appsettings.json` sections: `ConnectionStrings`, `Jwt`, `Smtp` — all values from env vars

### .NET Endpoints — AuthController
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/auth/register/customer` | None | Hash password, create customer, `tc_accepted=false` |
| POST | `/auth/register/operator` | None | Hash password, create operator with `status='PENDING'`, `tc_accepted=false` |
| POST | `/auth/login` | None | Detect role from DB, return JWT with `sub`, `role`, `email` claims |
| POST | `/auth/accept-tc` | Any role | Update `tc_version` + `tc_accepted_at` for caller |
| GET | `/auth/me` | Any role | Return current user profile decoded from JWT |

### Angular — AuthModule
- `AuthService`: login, register, JWT stored in `HttpOnly` cookie (set by server), expose `currentUser$` as `BehaviorSubject` (populated via `GET /auth/me` on app init)
- `JwtInterceptor`: sets `withCredentials: true` on every outgoing HTTP request so the browser sends the cookie automatically — Angular never reads the token directly
- Route guards: `CustomerGuard`, `OperatorGuard`, `AdminGuard` — read role from `currentUser$`
- Pages: Login, Register (with role selector), T&C modal (blocks all booking actions if T&C version mismatch)

### Sprint 1 Done When
- All 3 roles can register and login
- JWT is returned with correct role claim
- Protected routes reject wrong roles with 401/403
- T&C acceptance endpoint updates DB correctly

---

## Sprint 2 — Admin Core: Routes & Approvals
**Modules:** M-02 · M-10 (partial)  
**Depends on:** Sprint 1 (JWT auth must work)

### .NET Endpoints — AdminController
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/admin/routes` | Admin | Create city-pair route |
| GET | `/routes` | None | List all active routes (used by operator bus registration) |
| PATCH | `/admin/routes/{id}/toggle` | Admin | Enable or disable a route |
| GET | `/admin/operators/pending` | Admin | List operators with `status='PENDING'` |
| PATCH | `/admin/operators/{id}/approve` | Admin | Set `status='APPROVED'`, record `approved_by_admin`, write audit log |
| PATCH | `/admin/operators/{id}/reject` | Admin | Set `status='REJECTED'`, write audit log |
| PATCH | `/admin/operators/{id}/toggle` | Admin | Enable or disable an approved operator |
| GET | `/admin/buses/pending` | Admin | List buses awaiting approval |
| PATCH | `/admin/buses/{id}/approve` | Admin | Set bus `status='ACTIVE'`, write audit log |
| PATCH | `/admin/buses/{id}/reject` | Admin | Set bus `status='PENDING'` with rejection reason, write audit log |

### Angular — AdminModule (skeleton)
- Admin layout with sidebar navigation: Routes · Operators · Buses · Revenue
- Route management page: form to add city pairs, table of existing routes with toggle
- Operator queue page: table of PENDING operators with Approve / Reject buttons
- Bus queue page: table of PENDING buses with details and Approve / Reject buttons

### Sprint 2 Done When
- Admin can create routes and they appear in the public route list
- Admin can approve and reject operators end-to-end
- Admin can approve and reject buses end-to-end
- All approval actions are written to `audit_log`

---

## Sprint 3 — Operator Core: Bus & Layout Setup
**Modules:** M-03 · M-09 (partial)  
**Depends on:** Sprint 2 (routes must exist for bus registration)

### seat_config JSONB Format — Lock This In Before Coding
Define and agree on this shape before writing any code. Both the .NET layout parser and the Angular seat map renderer depend on it.
```json
{
  "rows": 10,
  "cols": 4,
  "decks": ["lower", "upper"],
  "seats": [
    { "seat_number": 1, "row": 1, "col": 1, "type": "window", "deck": "lower" },
    { "seat_number": 2, "row": 1, "col": 2, "type": "aisle", "deck": "lower" }
  ]
}
```

### .NET Endpoints — OperatorController
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/operator/layouts` | Operator | Create a bus layout with `seat_config` JSONB |
| GET | `/operator/layouts` | Operator | List own layouts |
| POST | `/operator/buses` | Operator | Register bus on existing route with `status='PENDING'` |
| POST | `/operator/buses/{id}/boarding-points` | Operator | Add boarding stop (`type='BOARDING'`) for a city |
| POST | `/operator/buses/{id}/dropping-points` | Operator | Add dropping stop (`type='DROPPING'`) for a city |
| PATCH | `/operator/buses/{id}/price` | Operator | Update `base_price` |
| PATCH | `/operator/buses/{id}/disable` | Operator | Set `status='DISABLED'`, trigger cancellation flow for future bookings |
| DELETE | `/operator/buses/{id}` | Operator | Soft delete — set `status='REMOVED'`, trigger cancellation flow |
| GET | `/operator/buses` | Operator | List all buses for this operator with status |
| GET | `/operator/bookings` | Operator | All bookings across operator's buses |

### Angular — OperatorModule
- Bus registration form: number plate, bus name, owner, route picker, layout picker
- Seat layout builder: visual grid rendered from `seat_config` JSON; operator clicks cells to configure
- Boarding/dropping point form: per city, address, scheduled time
- Bus list: status chips (PENDING / ACTIVE / DISABLED), disable and remove actions
- Bookings table: filterable by bus, shows passenger names and seats

### Sprint 3 Done When
- Operator can register a bus and it appears in the admin pending queue
- Admin approves it and the bus status becomes ACTIVE
- Boarding and dropping points are saved and retrievable

---

## Sprint 4 — Search, Seat Map & Locking
**Modules:** M-04 · M-05  
**Depends on:** Sprint 3 (active buses must exist)

### .NET — Search & Availability
**Seat availability query — write as a single SQL join, not three separate queries:**
```sql
SELECT s.id, s.seat_number, s.seat_type, s.deck,
  CASE
    WHEN bs.id IS NOT NULL THEN 'BOOKED'
    WHEN sl.id IS NOT NULL THEN 'LOCKED'
    ELSE 'AVAILABLE'
  END AS status,
  bs.passenger_gender
FROM seat s
LEFT JOIN booked_seat bs ON bs.seat_id = s.id AND bs.bus_id = @busId AND bs.journey_date = @date
LEFT JOIN seat_lock sl ON sl.seat_id = s.id AND sl.bus_id = @busId AND sl.journey_date = @date AND sl.is_active = true
WHERE s.layout_id = (SELECT layout_id FROM bus WHERE id = @busId)
```

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/buses/search` | None | `?from=&to=&date=` — returns active buses with available seat count |
| GET | `/buses/{id}/seats` | None | `?date=` — returns full seat layout with per-seat status and booked gender |
| GET | `/cities/autocomplete` | None | `?q=` — fuzzy city search using `pg_trgm` ILIKE |
| POST | `/seats/lock` | Customer | Insert into `seat_lock`, `expires_at = now() + interval '10 minutes'` |
| DELETE | `/seats/lock/{id}` | Customer | Release lock early (user navigates away) |

### .NET — Background Job (Seat Lock Expiry)
```csharp
// Implement as IHostedService using PeriodicTimer
// Runs every 60 seconds
// UPDATE seat_lock SET is_active = false WHERE expires_at < NOW() AND is_active = true
```

### Angular — Search & Seat UI
- Home page: source/destination autocomplete (calls `/cities/autocomplete`), date picker, optional return toggle
- Results page: bus cards showing operator, departure time, boarding/dropping points, price, seat count
- Seat map component:
  - Renders grid from `seat_config` JSON
  - Color coding: **blue** = male booked · **pink** = female booked · **gray** = locked · **white** = available · **yellow border** = selected
  - Calls `POST /seats/lock` on seat selection
  - Shows 10-minute countdown timer; on expiry releases lock and resets selection
- Login modal triggered when unauth user clicks Book

### Sprint 4 Done When
- Guest can search buses by route and date
- Seat map renders correctly with real availability data
- Seat lock is created on selection; background job expires stale locks
- Login modal triggers correctly for unauthenticated users

---

## Sprint 5 — Booking, Payment & Ticket Generation
**Modules:** M-06 · M-07 (partial)  
**Depends on:** Sprint 4 (seat locks must work)

### .NET — BookingController
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/bookings` | Customer | Create booking `INITIATED`, calculate fares, create `booked_seat` rows. Idempotent — check existing booking for same seat+date before insert |
| POST | `/bookings/{id}/pay` | Customer | Move to `PAYMENT_PENDING`, call dummy payment, on success → `CONFIRMED`. On failure → release lock, revert state |
| GET | `/bookings/mine` | Customer | Customer's full booking history |
| GET | `/bookings/{id}/ticket` | Customer | Stream PDF ticket |

### Fare Calculation Logic
```
base_fare     = bus.base_price × seat_count
convenience_fee = configured_flat_or_percent  (read from appsettings)
total_amount  = base_fare + convenience_fee - coupon_discount
```

### PDF Ticket — Required Fields
- Booking ID (use first 8 chars as PNR)
- Bus name and number plate
- Operator company name
- Journey date
- Boarding point: city + address + scheduled time
- Dropping point: city + address + scheduled time
- Per passenger: name, age, gender, seat number, deck
- Total amount paid
- Busly branding / footer

### .NET — Email Service
- Create `IEmailService` backed by `MailKit`
- Queue emails using `System.Threading.Channels.Channel<T>` — never block the HTTP response waiting for SMTP
- On `CONFIRMED`: send booking confirmation email with PDF ticket as attachment
- Insert row into `notification` table with `status='SENT'` or `'FAILED'`

### Angular — Booking Flow UI
- Booking summary page: selected seats, passenger detail form (name/age/gender per seat), coupon code field, fare breakdown table
- Dummy payment page: accepts any card input, Pay button calls `/bookings/{id}/pay`
- Confirmation page: PNR, Download Ticket button, "Email sent to {email}" notice
- My Bookings page: list with status chips, Download Ticket per confirmed booking

### Sprint 5 Done When
- Full happy path works end-to-end: search → seat lock → pay → CONFIRMED status → PDF download → email received
- Failed payment releases lock and booking stays INITIATED
- Idempotency: re-submitting the same booking request does not create a duplicate

---

## Sprint 6 — Cancellation, Refunds, Revenue & Dashboards
**Modules:** M-08 · M-07 (full) · M-09 (full) · M-10 (full)  
**Depends on:** Sprint 5 (confirmed bookings must exist)

### Business Rules — Cancellation Refund Calculation
| Time Before Departure | Triggered By | Refund |
|-----------------------|--------------|--------|
| More than 24 hours | Customer | 85% of `base_fare` |
| 12 to 24 hours | Customer | 50% of `base_fare` |
| Less than 12 hours | Customer | 0% |
| Bus removed or disabled | Operator | 100% of `total_amount` |

Convenience fee is **not refunded** on customer-triggered cancellations. It **is refunded** on operator-triggered cancellations (full `total_amount`).

### .NET — CancellationController
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/bookings/{id}/cancel` | Customer | Apply time-based refund rule, insert `cancellation` record, set booking `status='CANCELLED'`, send SMTP |
| POST | `/bookings/{id}/refund-complete` | System | Internal — set `status='REFUNDED'` after dummy refund processed |

### .NET — Operator Bus Removal Cascade
When bus status is set to `REMOVED` or `DISABLED`:
1. Find all future bookings where `bus_id = this bus` and `status = 'CONFIRMED'`
2. For each: insert `cancellation` (`cancelled_by='operator'`, `refund_amount = total_amount`, `refund_status='PENDING'`)
3. Set booking `status = 'CANCELLED'`
4. Generate coupon: unique code, `discount_type='flat'` or `'percent'`, `expires_at = now() + 30 days`, `issued_to_customer = booking.customer_id`
5. Send SMTP to each affected customer: cancellation notice + coupon code

### .NET — CouponController
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/bookings/apply-coupon` | Customer | Validate coupon code, check not expired and not used, return discount amount |
| (internal) | — | System | Mark `is_used=true` atomically when booking moves to CONFIRMED |

### .NET — Admin Revenue Endpoints
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/admin/revenue` | Admin | Sum of `convenience_fee` across CONFIRMED bookings, grouped by month |
| GET | `/admin/revenue/by-operator` | Admin | Per-operator: booking count + total fare + total convenience fee |

### Angular — Final UI
- My Bookings: Cancel button visible only on CONFIRMED bookings with departure > 12h; shows calculated refund before confirming
- Admin Revenue page: total revenue card, monthly chart, per-operator table
- Operator Dashboard: full bookings table per bus, bus enable/disable toggle, boarding point editor
- Coupon field on booking summary page with live discount preview

### Sprint 6 Done When
- Customer cancellation applies correct refund percentage for all three time windows
- Operator bus removal cancels all future bookings and issues coupons automatically
- Admin revenue page shows correct totals broken down by convenience fee
- Coupon codes apply correctly and cannot be reused

---

## Packages Reference

### .NET
| Package | Purpose |
|---------|---------|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | PostgreSQL EF Core provider |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT middleware |
| `BCrypt.Net-Next` | Password hashing |
| `MailKit` | SMTP email sending |
| `QuestPDF` | Server-side PDF ticket generation |
| `Swashbuckle.AspNetCore` | Swagger UI |

### Angular
| Package | Purpose |
|---------|---------|
| `@angular/material` or `bootstrap` | UI component library |
| `@auth0/angular-jwt` | JWT decode helper |

---

## Key Architectural Decisions

### Seat Availability — Single Query Rule
Never calculate seat availability with multiple round trips to the DB. One JOIN across `seat`, `booked_seat`, and `seat_lock` per request. See the SQL in Sprint 4.

### Seat Lock Expiry — Background Job
Implement as `IHostedService` using `PeriodicTimer` (runs every 60 seconds). Do not expire locks inline on the seat map API — that creates race conditions.

### Idempotency on Booking Creation
Before inserting a new booking, check: does a `CONFIRMED` or `PAYMENT_PENDING` booking already exist for this `customer_id` + `seat_id` + `journey_date`? If yes, return the existing booking. This prevents duplicate charges on network retries.

### Email Queue — Never Block HTTP
Use `System.Threading.Channels.Channel<EmailMessage>` as an in-memory queue. The HTTP response returns immediately; a background `IHostedService` drains the channel and calls SMTP. On failure, update `notification.status = 'FAILED'` for retry visibility.

### T&C Version Check
On every booking attempt (not on every login), the backend checks: `customer.tc_version == latest published tc_version`. If not, return `403` with code `TC_REACCEPTANCE_REQUIRED`. Angular intercepts this and shows the T&C modal before retrying.

### Logical Deletes
`booking`, `payment`, and `cancellation` rows are **never deleted**. Use `status` field transitions only. The `bus` table uses `status='REMOVED'` instead of `DELETE`.

---

## Folder Structure Suggestion

### .NET
```
Busly.API/
  Controllers/        AuthController, BookingController, AdminController, OperatorController, ...
  Models/             EF Core entity classes (one per table)
  DTOs/               Request and response shape classes
  Repositories/       Interface + implementation per entity group
  Services/           Business logic — BookingService, EmailService, CancellationService, ...
  BackgroundJobs/     SeatLockExpiryJob, EmailDispatchJob
  Helpers/            JwtHelper, RefundCalculator, CouponGenerator
  Program.cs
  appsettings.json
```

### Angular
```
src/app/
  core/               AuthService, JwtInterceptor, Guards
  shared/             Shared components (seat map, ticket card, navbar)
  features/
    auth/             Login, Register, TC modal
    search/           Home search, Results page
    booking/          Seat selection, Summary, Payment, Confirmation
    my-bookings/      History, Cancel, Download ticket
    operator/         Dashboard, Bus management, Layout builder
    admin/            Routes, Approvals, Revenue
```

---

*Last updated: Apr 23, 2026 · Busly v1.0 · Solo development build*