# Requirements Document

## Introduction

Busly is a full-stack online bus ticket booking platform connecting passengers with bus operators through a managed marketplace. The system is built on Angular (frontend), .NET Web API (backend), and PostgreSQL (database). It supports three distinct roles — Customer (Passenger), Bus Operator, and Admin — each with dedicated workflows, access controls, and dashboards. The platform handles the complete lifecycle from route discovery and seat selection through payment, ticket generation, and cancellation/refund processing.

---

## Glossary

- **System**: The Busly platform as a whole (frontend + backend + database).
- **API**: The .NET Web API backend.
- **Customer**: An authenticated end-user who searches for and books bus tickets.
- **Guest**: An unauthenticated visitor who may browse and search but cannot book.
- **Operator**: A Bus Operator company account that registers and manages buses.
- **Admin**: The platform superuser with governance and oversight responsibilities.
- **JWT**: JSON Web Token used for stateless authentication and role claims.
- **RBAC**: Role-Based Access Control enforced on all protected API endpoints.
- **Route**: An Admin-defined city-pair (source city → destination city) that buses operate on.
- **Bus**: An Operator-registered vehicle bound to a Route and a Bus_Layout.
- **Bus_Layout**: A reusable seat configuration template stored as JSONB.
- **Seat**: An individual seat derived from a Bus_Layout, with a seat number, type, and deck.
- **Bus_Stop**: An Operator-defined boarding or dropping location for a specific bus in a specific city.
- **Seat_Lock**: A temporary reservation of a seat during the payment grace period (10 minutes).
- **Booking**: A core transaction record linking a Customer to one or more seats on a Bus for a journey date.
- **Booked_Seat**: A per-seat passenger detail record linked to a Booking.
- **Payment**: A payment record linked 1:1 to a Booking.
- **Cancellation**: A record of a booking cancellation with refund amount and trigger actor.
- **Coupon**: A discount code issued to a Customer when an Operator cancels their booking.
- **Notification**: An SMTP email dispatch log entry per Customer or Operator.
- **Audit_Log**: An immutable record of all critical admin and system actions.
- **TC_Version**: A published version of the platform Terms & Conditions.
- **PNR**: Passenger Name Record — the first 8 characters of a Booking ID used as a human-readable reference.
- **Convenience_Fee**: A configurable platform fee (flat or percentage) added to every booking.
- **Departure_Time**: The scheduled departure time of the bus from the boarding point.
- **SMTP**: Simple Mail Transfer Protocol used for sending email notifications.
- **EF_Core**: Entity Framework Core — the ORM used for database access in the .NET backend.
- **Background_Job**: An `IHostedService` running on a periodic timer within the .NET API process.

---

## Requirements

### Requirement 1: User Registration and Authentication

**User Story:** As a visitor, I want to register and log in with my role, so that I can access role-specific features of the platform.

#### Acceptance Criteria

1. WHEN a Customer registration request is submitted with a unique email and password, THE API SHALL create a Customer record with a BCrypt-hashed password and `tc_accepted = false`.
2. WHEN an Operator registration request is submitted with a unique email and password, THE API SHALL create a Bus_Operator record with a BCrypt-hashed password, `status = 'PENDING'`, and `tc_accepted = false`.
3. WHEN a login request is submitted with valid credentials, THE API SHALL return a JWT containing the claims `sub` (user ID), `role` (Admin / Operator / Customer), and `email`.
4. WHEN a login request is submitted with invalid credentials, THE API SHALL return a 401 Unauthorized response.
5. THE API SHALL store JWT secrets, database connection strings, and SMTP credentials exclusively in environment variables — never in source code.
6. THE System SHALL store the JWT in memory on the Angular client and SHALL NOT persist it to localStorage or sessionStorage.

---

### Requirement 2: Role-Based Access Control

**User Story:** As a platform operator, I want every API endpoint to enforce role-based access, so that users can only perform actions permitted to their role.

#### Acceptance Criteria

1. THE API SHALL enforce role-based access control on every protected endpoint, returning 401 for unauthenticated requests and 403 for requests with an insufficient role.
2. WHEN a Customer JWT is used to call an Admin-only endpoint, THE API SHALL return a 403 Forbidden response.
3. WHEN an Operator JWT is used to call a Customer-only endpoint, THE API SHALL return a 403 Forbidden response.
4. THE Angular_Client SHALL implement route guards (`CustomerGuard`, `OperatorGuard`, `AdminGuard`) that read the role claim from the JWT and redirect unauthorized navigation attempts to the login page.
5. THE Angular_Client SHALL attach an `Authorization: Bearer <token>` header to every outgoing HTTP request via a JWT interceptor.

---

### Requirement 3: Terms & Conditions Acceptance

**User Story:** As a platform administrator, I want all users to accept the current Terms & Conditions before transacting, so that legal compliance is maintained.

#### Acceptance Criteria

1. WHEN a Customer or Operator submits a T&C acceptance request, THE API SHALL update the caller's `tc_version` and `tc_accepted_at` fields with the current active TC_Version and the current timestamp.
2. WHEN a Customer attempts to initiate a booking and their stored `tc_version` does not match the latest active TC_Version, THE API SHALL return a 403 response with the error code `TC_REACCEPTANCE_REQUIRED`.
3. WHEN the Angular_Client receives a `TC_REACCEPTANCE_REQUIRED` response, THE Angular_Client SHALL display the T&C modal and block the booking flow until the Customer accepts.
4. THE System SHALL retain all historical T&C acceptance records (version and timestamp) and SHALL NOT overwrite prior acceptance entries.
5. WHEN an Admin publishes a new TC_Version with `is_active = true`, THE System SHALL set all previously active TC_Version records to `is_active = false`.

---

### Requirement 4: Admin Route Management

**User Story:** As an Admin, I want exclusive control over the city-pair route network, so that Operators can only register buses on approved routes.

#### Acceptance Criteria

1. WHEN an Admin submits a route creation request with a unique source city and destination city, THE API SHALL create a Route record with `is_active = true` and record the `created_by_admin` reference.
2. IF a route creation request contains a source city and destination city combination that already exists, THEN THE API SHALL return a 409 Conflict response.
3. WHEN an Admin toggles a route, THE API SHALL update the Route's `is_active` field to the opposite of its current value.
4. THE API SHALL expose a public (unauthenticated) endpoint that returns all active routes, used by Operators during bus registration.
5. THE API SHALL write an Audit_Log entry for every route creation and toggle action, recording the Admin's ID, the action name, the entity type, and the timestamp.

---

### Requirement 5: Operator Account Approval

**User Story:** As an Admin, I want to approve or reject Operator registrations, so that only vetted companies can list buses on the platform.

#### Acceptance Criteria

1. THE API SHALL provide an Admin-only endpoint that returns all Bus_Operator records with `status = 'PENDING'`.
2. WHEN an Admin approves an Operator, THE API SHALL set the Operator's `status` to `'APPROVED'`, record `approved_by_admin` and `approved_at`, and write an Audit_Log entry.
3. WHEN an Admin rejects an Operator, THE API SHALL set the Operator's `status` to `'REJECTED'` and write an Audit_Log entry.
4. WHEN an Admin toggles an Operator account, THE API SHALL set the Operator's `status` to `'DISABLED'` if currently `'APPROVED'`, or to `'APPROVED'` if currently `'DISABLED'`.
5. WHILE an Operator's `status` is not `'APPROVED'`, THE API SHALL reject all Operator-authenticated requests to manage buses with a 403 Forbidden response.

---

### Requirement 6: Bus Registration and Layout Management

**User Story:** As an Operator, I want to register buses with custom seat layouts and assign them to routes, so that passengers can discover and book seats on my services.

#### Acceptance Criteria

1. WHEN an Operator submits a bus layout creation request with a valid `seat_config` JSONB payload, THE API SHALL create a Bus_Layout record and generate individual Seat records for each seat defined in the `seat_config`.
2. THE `seat_config` JSONB SHALL conform to the schema: `{ "rows": int, "cols": int, "decks": [string], "seats": [{ "seat_number": int, "row": int, "col": int, "type": string, "deck": string }] }`.
3. WHEN an Operator submits a bus registration request referencing a valid Route and Bus_Layout, THE API SHALL create a Bus record with `status = 'PENDING'` and link it to the Operator.
4. WHEN an Operator adds a boarding or dropping point for a bus, THE API SHALL create a Bus_Stop record with the correct `type` (`'BOARDING'` or `'DROPPING'`), city, address, and scheduled time.
5. WHEN an Operator updates a bus price, THE API SHALL update the Bus record's `base_price` field.
6. WHEN an Operator disables a bus, THE API SHALL set the Bus `status` to `'DISABLED'` and trigger the Operator Cancellation Cascade (see Requirement 14).
7. WHEN an Operator removes a bus, THE API SHALL set the Bus `status` to `'REMOVED'` and trigger the Operator Cancellation Cascade (see Requirement 14).

---

### Requirement 7: Bus Approval by Admin

**User Story:** As an Admin, I want to approve or reject bus submissions, so that only compliant vehicles are listed for booking.

#### Acceptance Criteria

1. THE API SHALL provide an Admin-only endpoint that returns all Bus records with `status = 'PENDING'`.
2. WHEN an Admin approves a bus, THE API SHALL set the Bus `status` to `'ACTIVE'`, record `approved_by_admin` and `approved_at`, and write an Audit_Log entry.
3. WHEN an Admin rejects a bus, THE API SHALL set the Bus `status` to `'PENDING'` with a rejection reason and write an Audit_Log entry.
4. WHILE a Bus `status` is not `'ACTIVE'`, THE System SHALL exclude that bus from all public search results.

---

### Requirement 8: Public Bus Search and Discovery

**User Story:** As a Guest or Customer, I want to search for buses by route and date without logging in, so that I can browse available services before committing to a booking.

#### Acceptance Criteria

1. THE API SHALL expose a public (unauthenticated) search endpoint that accepts `from`, `to`, and `date` query parameters and returns all active buses matching the route and date.
2. WHEN a search request is processed, THE API SHALL include the available seat count for each bus in the response, calculated from the difference between total seats and confirmed or locked seats for that journey date.
3. THE API SHALL expose a public fuzzy city autocomplete endpoint that accepts a query string and returns matching city names using PostgreSQL `pg_trgm` ILIKE matching.
4. THE Angular_Client SHALL call the autocomplete endpoint on each keystroke in the source and destination fields and display matching suggestions.
5. WHILE a bus has `status != 'ACTIVE'`, THE API SHALL exclude it from all search results.

---

### Requirement 9: Seat Map and Availability

**User Story:** As a Customer, I want to view an interactive seat map with real-time availability, so that I can choose my preferred seat before booking.

#### Acceptance Criteria

1. THE API SHALL expose a public endpoint that returns the full seat layout for a given bus and journey date, with each seat's status (`AVAILABLE`, `LOCKED`, or `BOOKED`) and the booked passenger's gender where applicable.
2. THE API SHALL calculate seat availability using a single SQL JOIN across the `seat`, `booked_seat`, and `seat_lock` tables — not via multiple sequential queries.
3. THE Angular_Client SHALL render the seat map as a grid derived from the `seat_config` JSON, applying color coding: blue for male-booked seats, pink for female-booked seats, gray for locked seats, white for available seats, and yellow border for selected seats.
4. WHEN a Guest clicks a seat to book, THE Angular_Client SHALL display a login modal before proceeding.

---

### Requirement 10: Seat Locking

**User Story:** As a Customer, I want my selected seats to be temporarily reserved while I complete payment, so that another user cannot book the same seat during my checkout.

#### Acceptance Criteria

1. WHEN a Customer selects a seat, THE API SHALL insert a Seat_Lock record with `expires_at = NOW() + 10 minutes` and `is_active = true`.
2. IF a seat already has an active Seat_Lock or a confirmed Booked_Seat for the same bus and journey date, THEN THE API SHALL return a 409 Conflict response.
3. WHEN a Customer navigates away from the seat selection, THE Angular_Client SHALL call the seat lock release endpoint to delete the active lock early.
4. THE Background_Job SHALL run every 60 seconds and set `is_active = false` on all Seat_Lock records where `expires_at < NOW()` and `is_active = true`.
5. THE Angular_Client SHALL display a 10-minute countdown timer during seat selection; WHEN the timer expires, THE Angular_Client SHALL release the lock and reset the seat selection.

---

### Requirement 11: Booking Creation

**User Story:** As a Customer, I want to create a booking for my selected seats with passenger details, so that I have a confirmed reservation record.

#### Acceptance Criteria

1. WHEN a Customer submits a booking creation request, THE API SHALL create a Booking record with `status = 'INITIATED'`, calculate `base_fare = bus.base_price × seat_count`, apply the configured Convenience_Fee, and create one Booked_Seat record per seat with the provided passenger name, age, and gender.
2. THE Convenience_Fee SHALL be read from platform configuration and may be either a flat amount or a percentage of `base_fare`.
3. WHEN a booking creation request is received for a `customer_id` + `seat_id` + `journey_date` combination that already has a `CONFIRMED` or `PAYMENT_PENDING` booking, THE API SHALL return the existing booking record without creating a duplicate.
4. WHEN a Customer applies a coupon code during booking, THE API SHALL validate that the coupon is not expired, not already used, and issued to the requesting Customer before applying the discount.
5. THE total_amount SHALL be calculated as: `base_fare + convenience_fee - coupon_discount`.

---

### Requirement 12: Payment Processing

**User Story:** As a Customer, I want to pay for my booking through a simulated payment flow, so that my booking is confirmed and my seats are secured.

#### Acceptance Criteria

1. WHEN a Customer initiates payment for a booking, THE API SHALL set the Booking `status` to `'PAYMENT_PENDING'` and create a Payment record with `status = 'PENDING'`.
2. WHEN the dummy payment gateway returns a success response, THE API SHALL set the Booking `status` to `'CONFIRMED'`, set the Payment `status` to `'SUCCESS'`, record `paid_at`, and mark any applied coupon as `is_used = true` atomically.
3. WHEN the dummy payment gateway returns a failure response, THE API SHALL set the Payment `status` to `'FAILED'`, release all associated Seat_Locks, and revert the Booking `status` to `'INITIATED'`.
4. THE Booking state machine SHALL only allow the following transitions: `INITIATED → PAYMENT_PENDING → CONFIRMED → CANCELLED → REFUNDED`. Any other transition SHALL be rejected.
5. WHEN a payment times out without a response, THE API SHALL treat it as a failure and apply the same failure handling as criterion 3.

---

### Requirement 13: Ticket Generation and Email Notification

**User Story:** As a Customer, I want to receive a PDF ticket by email after a confirmed booking, so that I have proof of my reservation.

#### Acceptance Criteria

1. WHEN a Booking transitions to `CONFIRMED`, THE API SHALL enqueue an email job containing a PDF ticket attachment and send it to the Customer's registered email address.
2. THE PDF_Ticket SHALL include: PNR (first 8 characters of Booking ID), bus name, bus number plate, Operator company name, journey date, boarding point (city, address, scheduled time), dropping point (city, address, scheduled time), per-passenger details (name, age, gender, seat number, deck), total amount paid, and Busly platform branding.
3. THE Email_Service SHALL use an in-memory `System.Threading.Channels.Channel<T>` queue and a Background_Job to dispatch emails asynchronously — the HTTP response SHALL return before SMTP delivery completes.
4. THE API SHALL insert a Notification record for each email dispatch attempt with `status = 'SENT'` on success or `status = 'FAILED'` on SMTP error.
5. THE API SHALL expose an endpoint that streams the PDF ticket for a confirmed booking, accessible only by the owning Customer.

---

### Requirement 14: Operator-Triggered Cancellation Cascade

**User Story:** As a Customer, I want to be automatically refunded and compensated when an Operator cancels my booking by disabling or removing a bus, so that I am not financially disadvantaged.

#### Acceptance Criteria

1. WHEN a Bus `status` is set to `'DISABLED'` or `'REMOVED'`, THE API SHALL find all future Bookings for that bus with `status = 'CONFIRMED'` and process the Operator Cancellation Cascade for each.
2. FOR EACH affected Booking in the cascade, THE API SHALL insert a Cancellation record with `cancelled_by = 'operator'`, `refund_amount = booking.total_amount`, and `refund_status = 'PENDING'`, then set the Booking `status` to `'CANCELLED'`.
3. FOR EACH affected Booking in the cascade, THE API SHALL generate a Coupon with a unique code, `discount_type = 'flat'`, `expires_at = NOW() + 30 days`, and `issued_to_customer = booking.customer_id`.
4. FOR EACH affected Booking in the cascade, THE API SHALL enqueue an SMTP notification to the Customer containing the cancellation notice and the issued Coupon code.
5. THE API SHALL process the entire cascade within a single database transaction to ensure atomicity.

---

### Requirement 15: Customer-Triggered Cancellation and Refunds

**User Story:** As a Customer, I want to cancel my confirmed booking and receive a time-based refund, so that I can recover part of my fare if my plans change.

#### Acceptance Criteria

1. WHEN a Customer submits a cancellation request for a `CONFIRMED` booking, THE API SHALL calculate the refund amount based on the time remaining until the bus departure: 85% of `base_fare` if more than 24 hours remain, 50% of `base_fare` if 12 to 24 hours remain, and 0% if fewer than 12 hours remain.
2. THE Convenience_Fee SHALL NOT be included in the refund amount for customer-triggered cancellations.
3. WHEN a cancellation is processed, THE API SHALL insert a Cancellation record with `cancelled_by = 'customer'`, the calculated `refund_amount`, and `refund_status = 'PENDING'`, then set the Booking `status` to `'CANCELLED'`.
4. WHEN a cancellation is processed, THE API SHALL enqueue an SMTP notification to the Customer confirming the cancellation and the refund amount.
5. THE Angular_Client SHALL display the Cancel button only for `CONFIRMED` bookings where the departure time is more than 12 hours away, and SHALL show the calculated refund amount before the Customer confirms the cancellation.

---

### Requirement 16: Admin Revenue Reporting

**User Story:** As an Admin, I want to view aggregate platform revenue broken down by month and by Operator, so that I can monitor the financial health of the platform.

#### Acceptance Criteria

1. THE API SHALL provide an Admin-only endpoint that returns the sum of `convenience_fee` across all `CONFIRMED` bookings, grouped by calendar month.
2. THE API SHALL provide an Admin-only endpoint that returns per-Operator metrics: total booking count, total `base_fare` collected, and total `convenience_fee` collected.
3. THE Angular_Admin_Dashboard SHALL display a total revenue summary card, a monthly revenue chart, and a per-Operator breakdown table.

---

### Requirement 17: Operator Dashboard

**User Story:** As an Operator, I want a dashboard showing all bookings across my buses and tools to manage my fleet, so that I can run my operations efficiently.

#### Acceptance Criteria

1. THE API SHALL provide an Operator-authenticated endpoint that returns all Bookings across all buses owned by the requesting Operator.
2. THE Angular_Operator_Dashboard SHALL display a bookings table filterable by bus, showing passenger names and seat numbers.
3. THE Angular_Operator_Dashboard SHALL display bus status chips (`PENDING`, `ACTIVE`, `DISABLED`) and provide disable and remove action controls.
4. THE Angular_Operator_Dashboard SHALL provide a boarding and dropping point editor for each bus.

---

### Requirement 18: Audit Logging

**User Story:** As a platform administrator, I want all critical actions to be logged with actor identity and timestamp, so that I have a complete audit trail for compliance and dispute resolution.

#### Acceptance Criteria

1. THE API SHALL write an Audit_Log entry for every critical action including: booking creation, payment updates, cancellation processing, Admin approvals and rejections, and route management changes.
2. EACH Audit_Log entry SHALL record: `actor_id`, `actor_role`, `action`, `entity_type`, `entity_id`, optional `metadata` (JSONB), and `performed_at` timestamp.
3. THE Audit_Log table SHALL be append-only — no Audit_Log record SHALL ever be updated or deleted.

---

### Requirement 19: Data Integrity and Logical Deletion

**User Story:** As a platform administrator, I want critical records to be preserved through status transitions rather than physical deletion, so that historical data is available for auditing and dispute resolution.

#### Acceptance Criteria

1. THE API SHALL NEVER issue a SQL `DELETE` statement against the `booking`, `payment`, or `cancellation` tables.
2. THE API SHALL manage the `bus` table lifecycle exclusively through `status` field transitions (`PENDING → ACTIVE → DISABLED / REMOVED`).
3. THE System SHALL enforce a unique constraint at the database level preventing two Booked_Seat records from sharing the same `seat_id`, `bus_id`, and `journey_date`.
4. THE System SHALL enforce all status field values through PostgreSQL `CHECK` constraints as defined in the schema.

---

### Requirement 20: System Configuration

**User Story:** As a platform administrator, I want platform-wide settings like the convenience fee to be configurable without code changes, so that business parameters can be adjusted at runtime.

#### Acceptance Criteria

1. THE API SHALL read the Convenience_Fee configuration (type: flat or percent, value) from the `platform_config` table or `appsettings.json` at application startup.
2. THE API SHALL read all secrets (database connection string, JWT secret, SMTP credentials) from environment variables.
3. WHERE the Convenience_Fee type is `'flat'`, THE API SHALL add the configured flat amount to every booking's `convenience_fee` field.
4. WHERE the Convenience_Fee type is `'percent'`, THE API SHALL calculate `convenience_fee = base_fare × (configured_percent / 100)` for every booking.

---

### Requirement 21: PNR Lookup with CAPTCHA

**User Story:** As a guest or customer, I want to look up a booking by its PNR code without logging in, so that I can check booking status and journey details.

#### Acceptance Criteria

1. THE API SHALL expose a public (unauthenticated) endpoint that accepts an 8-character PNR code and returns booking details.
2. BEFORE processing a PNR lookup, THE API SHALL require a valid CAPTCHA token and user-supplied CAPTCHA input to prevent automated scraping.
3. THE CAPTCHA SHALL expire after 5 minutes and allow a maximum of 3 validation attempts per session.
4. THE PNR lookup response SHALL NOT expose the customer's real name or email address — those fields SHALL return `"Private Information"`.
5. THE PNR lookup response SHALL include: booking status, journey date, source and destination cities, bus number, departure and arrival times, seat numbers, total amount, booked-at timestamp, cancellation details (if any), and a `canCancel` flag.
6. THE `canCancel` flag SHALL be `true` only when the booking is `CONFIRMED`, the journey date is in the future, and the departure is more than 12 hours away.

---

### Requirement 22: Bulk Seat Locking

**User Story:** As a Customer, I want to lock multiple seats in a single request, so that I can reserve all seats for my group without race conditions between individual lock calls.

#### Acceptance Criteria

1. THE API SHALL expose a bulk seat lock endpoint that accepts a list of seat IDs (maximum 4), a bus ID, and a journey date.
2. THE API SHALL attempt to lock each seat independently and return a response listing which seats were successfully locked and which failed.
3. IF all seats are successfully locked, THE response SHALL include `allSuccessful = true`.
4. IF any seat fails to lock (already locked or booked), THE API SHALL still lock the remaining available seats and report the failed seat IDs separately.

---

### Requirement 23: Seat Lock Security and Fraud Prevention

**User Story:** As a platform administrator, I want seat locking to be protected against abuse and bot activity, so that seats are not artificially held by malicious actors.

#### Acceptance Criteria

1. THE API SHALL enforce a rate limit of 5 seat lock attempts per minute per IP address.
2. THE API SHALL enforce a limit of 10 seat lock attempts per customer per hour.
3. THE API SHALL enforce a maximum of 4 active seat locks per customer at any time.
4. THE API SHALL detect and block suspicious activity patterns, including: more than 10 lock attempts from the same IP within 5 minutes, more than 3 distinct customer accounts from the same IP within 24 hours, and a single customer locking seats across more than 5 different buses within 1 hour.
5. THE `SeatLockCleanupService` background job SHALL run every 5 minutes and delegate expired lock cleanup to the `ISeatLockSecurityService`.

---

### Requirement 24: Bus Operating Days

**User Story:** As an Operator, I want to define which days of the week my bus operates, so that the search results only show my bus on days it actually runs.

#### Acceptance Criteria

1. THE API SHALL expose an Operator-authenticated endpoint to set the operating days for a bus, accepting a list of day-of-week values (1 = Monday through 7 = Sunday) with an `isActive` flag per day.
2. WHEN operating days are updated, THE API SHALL replace the existing operating day records for that bus with the new set.
3. THE `BusOperatingDay` entity SHALL store `bus_id`, `day_of_week`, `is_active`, `created_at`, and `updated_at`.

---

### Requirement 25: Journey Date Validation

**User Story:** As a platform administrator, I want the system to reject bookings and seat locks for invalid dates, so that customers cannot book for past dates or unreasonably far in the future.

#### Acceptance Criteria

1. THE API SHALL reject seat lock requests for journey dates in the past with an appropriate error message.
2. THE API SHALL reject seat lock requests for journey dates more than 90 days in the future.
3. THE API SHALL reject same-day seat lock requests submitted after 20:00 UTC.
4. THE API SHALL reject booking creation requests for journey dates that are today or in the past (bookings require at least next-day travel).
5. THE API SHALL reject booking creation requests for journey dates more than 90 days in the future.
