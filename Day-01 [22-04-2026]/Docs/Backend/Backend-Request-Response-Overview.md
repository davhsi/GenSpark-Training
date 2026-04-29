# Busly Backend: Request/Response + Architecture Overview

This document combines:
- API request/response behavior (what clients send and receive)
- Overall backend architecture (how the system is structured internally)

## 1) Tech Stack and Runtime

- **Backend framework:** ASP.NET Core Web API
- **Language/runtime:** C# on .NET
- **Database:** PostgreSQL (via EF Core + Npgsql)
- **Auth:** JWT bearer token stored in an HttpOnly cookie (`busly_token`)
- **API docs:** Swagger/OpenAPI in development mode

## 2) Startup Configuration (`Program.cs`)

The backend startup config performs the following:
- Reads `.env` values (falls back to `appsettings` when needed)
- Configures DB connection from `BUSLY_DB_CONNECTION`
- Configures JWT secret from `BUSLY_JWT_SECRET`
- Enables authentication and role-based authorization policies:
  - `Admin`
  - `Operator`
  - `Customer`
- Enables CORS for `http://localhost:4200` with credentials enabled
- Registers repositories, services, and hosted background jobs:
  - `SeatLockExpiryJob`
  - `EmailDispatchJob`

## 3) High-Level Backend Architecture

The project follows a layered architecture:

- **Controllers**
  - Own endpoint routing and HTTP status codes
  - Read claims from JWT (`sub`, role)
  - Convert service/repository outcomes into API responses

- **Services**
  - Hold business rules and workflows
  - Coordinate across repositories
  - Raise domain exceptions for invalid operations

- **Repositories**
  - Perform data access using EF Core
  - Encapsulate table/entity queries and updates

- **Data Layer**
  - PostgreSQL schema defines core entities: users, routes, buses, seats, bookings, locks, payments, cancellations, coupons, audits
  - Constraints and indexes enforce integrity and performance (for example: unique seat-per-trip index)

## 4) Authentication and Authorization Flow

### Login Flow
1. Client posts credentials to `POST /auth/login`.
2. On success, backend sets `busly_token` as HttpOnly cookie.
3. Response body returns identity context (`role`, `email`, `userId`) but not the raw JWT.

### Subsequent Requests
- JWT middleware reads token from the `busly_token` cookie.
- Endpoints with `[Authorize]` or policy checks validate role.
- Controllers extract user id from `sub` claim.

### Logout
- `POST /auth/logout` deletes the token cookie.

## 5) API Endpoint Request/Response Reference

> Status codes below reflect controller behavior and common outcomes.

## Auth Endpoints (`/auth`)

### `POST /auth/register/customer`
- **Request body:** `RegisterCustomerRequest`
- **Success:** `201 Created` with:
  - `message`
  - `userId`
- **Failures:**
  - `409 Conflict` when email is already registered

### `POST /auth/register/operator`
- **Request body:** `RegisterOperatorRequest`
- **Success:** `201 Created` with:
  - `message` (awaiting admin approval)
  - `userId`
- **Failures:**
  - `409 Conflict` when email is already registered

### `POST /auth/login`
- **Request body:** `LoginRequest`
- **Success:** `200 OK`, sets cookie, returns:
  - `role`
  - `email`
  - `userId`
- **Failures:**
  - `401 Unauthorized` invalid credentials

### `POST /auth/logout`
- **Success:** `200 OK` with success message; token cookie removed

### `POST /auth/accept-tc`
- **Auth required:** Any authenticated user
- **Success:** `200 OK`
- **Failures:** `401 Unauthorized` for invalid token claim

### `GET /auth/me`
- **Auth required**
- **Success:** `200 OK` with profile payload from service
- **Failures:** `401 Unauthorized` for invalid token claim

---

## Search and Seat Lock Endpoints

### `GET /buses/search`
- **Query params:** `from`, `to`, `date` (`yyyy-MM-dd`)
- **Success:** `200 OK` with matching buses
- **Failures:**
  - `400 Bad Request` for invalid date format

### `GET /buses/{id}/seats`
- **Query params:** `date` (`yyyy-MM-dd`)
- **Success:** `200 OK` with seat map
- **Failures:**
  - `400 Bad Request` for invalid date format

### `GET /cities/autocomplete`
- **Query params:** `q`
- **Success:** `200 OK` with city suggestions (`[]` if empty query)

### `POST /seats/lock`
- **Auth policy:** `Customer`
- **Request body:** `CreateSeatLockRequest`
- **Success:** `201 Created` with lock details
- **Failures:**
  - `401 Unauthorized` invalid token
  - `409 Conflict` seat already locked/booked

### `DELETE /seats/lock/{id}`
- **Auth policy:** `Customer`
- **Success:** `204 No Content`
- **Failures:**
  - `401 Unauthorized` invalid token
  - `403 Forbidden` lock owned by another user
  - `404 Not Found` lock does not exist

---

## Booking Endpoints (`/bookings`)

### `POST /bookings`
- **Auth policy:** `Customer`
- **Request body:** `CreateBookingRequest`
- **Success:** `201 Created` with booking payload
- **Failures:**
  - `401 Unauthorized` invalid token
  - `403 Forbidden` with `{ code: "TC_REACCEPTANCE_REQUIRED" }`
  - `400 Bad Request` invalid coupon

### `POST /bookings/{id}/pay`
- **Auth policy:** `Customer`
- **Success:** `200 OK` with updated booking/payment state
- **Failures:**
  - `401 Unauthorized` invalid token
  - `403 Forbidden` unauthorized booking access
  - `400 Bad Request` invalid payment state/rules

### `GET /bookings/mine`
- **Auth policy:** `Customer`
- **Success:** `200 OK` with customer booking list

### `GET /bookings/{id}/ticket`
- **Auth policy:** `Customer`
- **Success:** `200 OK` PDF file (`application/pdf`)
- **Failures:**
  - `403 Forbidden` unauthorized booking access
  - `400 Bad Request` ticket generation/rule error

---

## Coupon Endpoint

### `POST /bookings/apply-coupon`
- **Auth policy:** `Customer`
- **Request body:** `ApplyCouponRequest`
- **Success:** `200 OK` with:
  - `discountValue`
  - `discountType`
  - `code`
- **Failures:** `400 Bad Request` when coupon is:
  - missing
  - expired
  - already used
  - issued to a different customer

---

## Cancellation Endpoints

### `POST /bookings/{id}/cancel`
- **Auth policy:** `Customer`
- **Success:** `200 OK` with cancellation/refund preview or result
- **Failures:**
  - `404 Not Found` booking not found
  - `403 Forbidden` access denied
  - `400 Bad Request` invalid cancellation state/rules

### `POST /bookings/{id}/refund-complete`
- **Auth policy:** `Admin`
- **Success:** `200 OK` (`Refund processed`)
- **Failures:**
  - `404 Not Found` cancellation record not found

---

## Admin Endpoints

All `/admin/*` endpoints require `Admin` policy unless noted.

### Routes
- `POST /admin/routes`
  - Creates route
  - `201 Created` on success
  - `409 Conflict` when duplicate
- `GET /routes` (public)
  - `200 OK` active routes list
- `PATCH /admin/routes/{id}/toggle`
  - `200 OK` route toggled

### Operator Queue
- `GET /admin/operators/pending` -> `200 OK`
- `PATCH /admin/operators/{id}/approve` -> `200 OK`
- `PATCH /admin/operators/{id}/reject` -> `200 OK`
- `PATCH /admin/operators/{id}/toggle`
  - `200 OK` on success
  - `404 Not Found` if operator missing

### Bus Queue
- `GET /admin/buses/pending` -> `200 OK`
- `PATCH /admin/buses/{id}/approve` -> `200 OK`
- `PATCH /admin/buses/{id}/reject` -> `200 OK`

### Revenue
- `GET /admin/revenue` -> `200 OK`
- `GET /admin/revenue/by-operator` -> `200 OK`

---

## Operator Endpoints (`/operator`)

All require `Operator` policy.

### Layouts
- `POST /operator/layouts` -> `201 Created` (`CreateLayoutRequest`)
- `GET /operator/layouts` -> `200 OK`
- `DELETE /operator/layouts/{id}` -> `200 OK`
  - `409 Conflict` when business rule prevents deletion

### Buses
- `POST /operator/buses` -> `201 Created` (`RegisterBusRequest`)
- `GET /operator/buses` -> `200 OK`
- `GET /operator/buses/{id}` -> `200 OK` or `404 Not Found`
- `PATCH /operator/buses/{id}/price` -> `200 OK`
- `PATCH /operator/buses/{id}/staff` -> `200 OK`
- `PATCH /operator/buses/{id}/disable` -> `200 OK`
- `DELETE /operator/buses/{id}` -> `200 OK`

### Bus Stops
- `POST /operator/buses/{id}/boarding-points` -> `201 Created`
- `POST /operator/buses/{id}/dropping-points` -> `201 Created`
- `DELETE /operator/buses/stops/{id}` -> `200 OK`

## 6) Core Data Model Snapshot

Major tables defined in schema:
- `admin`, `customer`, `bus_operator`
- `route`, `bus_layout`, `seat`, `bus`, `bus_stop`
- `booking`, `booked_seat`, `seat_lock`, `payment`
- `cancellation`, `coupon`
- `notification`, `tc_version`, `audit_log`, `platform_config`

Important constraints/indexes:
- Unique route pair (`source_city`, `destination_city`)
- Unique booked seat per bus/date to prevent double-booking
- Seat lock lookup index for seat map/locking performance
- Trigram indexes for city autocomplete/search

## 7) Request/Response Conventions Used by This Backend

- JSON object responses for most endpoints
- Standard status code mapping:
  - `200`, `201`, `204` for success patterns
  - `400`, `401`, `403`, `404`, `409` for domain/auth/validation errors
- Error payload style is usually:
  - `{ "message": "..." }`
  - sometimes `{ "code": "..." }` for machine-readable client branching
- Authentication token is cookie-based, not exposed in response bodies

## 8) Suggested Next Improvements

- Add a dedicated API contract file from Swagger output for strict frontend typing.
- Standardize all error responses to a single envelope shape.
- Add endpoint-specific examples for each DTO in this document.
- Add versioning (`/v1`) when contracts stabilize.
