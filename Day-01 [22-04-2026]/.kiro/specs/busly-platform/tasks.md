# Tasks — Busly Platform

## Sprint 1 — Foundation: Auth & Role Setup

- [x] 1.1 DB: Run full SQL schema; verify all tables, indexes, and CHECK constraints; enable uuid-ossp and pg_trgm extensions
- [x] 1.2 EF Core Entity: Create AppDbContext with all entity models mapped to the schema
- [x] 1.3 Repository: Implement IAuthRepository / AuthRepository (customer, bus_operator, admin lookup by email; create customer; create operator)
- [x] 1.4 Service: Implement AuthService (BCrypt password hashing/verification, JWT generation with sub/role/email claims, role detection on login, T&C acceptance update, profile retrieval)
- [x] 1.5 Controller: Implement AuthController — POST /auth/register/customer, POST /auth/register/operator, POST /auth/login (JWT stored in HttpOnly cookie `busly_token`), POST /auth/logout (clear cookie), POST /auth/accept-tc, GET /auth/tc-status, GET /auth/me
- [x] 1.6 Angular Service: Implement AuthService (login, register, currentUser$ BehaviorSubject, JWT in-memory storage)
- [x] 1.7 Angular Component: Implement Login page, Register page (with role selector), JwtInterceptor, CustomerGuard, OperatorGuard, AdminGuard, T&C modal component

## Sprint 2 — Admin Core: Routes & Approvals

- [x] 2.1 DB: Verify route, audit_log, and bus_operator tables are ready; confirm GIN trigram indexes on route cities
- [x] 2.2 EF Core Entity: Confirm Route, AuditLog, BusOperator entities are correctly mapped with navigation properties
- [x] 2.3 Repository: Implement IRouteRepository / RouteRepository (create, list active, list all, toggle, get by id, get by cities, city suggestions); IAuditRepository / AuditRepository (append-only insert, get logs); extend IAuthRepository for operator status updates and bus status updates
- [x] 2.4 Service: Implement AdminService (route creation with uniqueness check, route toggle, operator approve/reject/toggle, bus approve/reject/toggle, revenue aggregation, T&C version management, audit log writes)
- [x] 2.5 Controller: Implement AdminController — POST /admin/routes, GET /routes, PATCH /admin/routes/{id}/toggle, GET /admin/operators/pending, GET /admin/operators, PATCH /admin/operators/{id}/approve, PATCH /admin/operators/{id}/reject, PATCH /admin/operators/{id}/toggle, GET /admin/buses/pending, GET /admin/buses, GET /admin/operators/{operatorId}/buses, PATCH /admin/buses/{id}/approve, PATCH /admin/buses/{id}/reject, PATCH /admin/buses/{id}/toggle, GET /admin/revenue, GET /admin/revenue/by-operator, GET /admin/tc, GET /tc/current, POST /admin/tc, GET /admin/audit-logs
- [x] 2.6 Angular Service: Implement AdminService (route CRUD, operator approval queue, bus approval queue)
- [x] 2.7 Angular Component: Implement Admin layout with sidebar, Route management page, Operator queue page, Bus queue page

## Sprint 3 — Operator Core: Bus & Layout Setup

- [x] 3.1 DB: Verify bus_layout, bus, seat, bus_stop, bus_operating_days tables; confirm seat_config JSONB column; confirm bus status CHECK constraint
- [x] 3.2 EF Core Entity: Confirm BusLayout (with JSONB seat_config), Bus, Seat, BusStop, BusOperatingDay entities; add SeatConfigDto C# class matching the locked JSONB schema
- [x] 3.3 Repository: Implement IBusRepository / BusRepository (create layout, list layouts, delete layout, create bus, add bus stop, remove bus stop, update price, update staff, update status, list by operator, operating days CRUD); ISeatRepository / SeatRepository (bulk insert seats from seat_config)
- [x] 3.4 Service: Implement OperatorService (layout creation + seat row generation, bus registration, boarding/dropping point management, price update, staff update, operating days update, bus disable/remove with cascade trigger, profile retrieval)
- [x] 3.5 Controller: Implement OperatorController — POST /operator/layouts, GET /operator/layouts, DELETE /operator/layouts/{id}, POST /operator/buses, GET /operator/buses, GET /operator/buses/{id}, POST /operator/buses/{id}/boarding-points, POST /operator/buses/{id}/dropping-points, DELETE /operator/buses/stops/{id}, PATCH /operator/buses/{id}/price, PATCH /operator/buses/{id}/staff, PATCH /operator/buses/{id}/disable, DELETE /operator/buses/{id}, PUT /operator/buses/{busId}/operating-days, GET /operator/bookings, GET /operator/profile
- [x] 3.6 Angular Service: Implement OperatorService (layout builder, bus registration, boarding/dropping point management, bus list)
- [x] 3.7 Angular Component: Implement Bus registration form, Seat layout builder (visual grid from seat_config), Boarding/dropping point form, Bus list with status chips

## Sprint 4 — Search, Seat Map & Locking

- [x] 4.1 DB: Verify seat_lock table and idx_seat_lock_lookup index; verify unique_seat_per_trip index on booked_seat
- [x] 4.2 EF Core Entity: Confirm SeatLock entity with all fields; confirm Seat and BookedSeat navigation properties
- [x] 4.3 Repository: Implement ISeatRepository methods for seat availability (single-JOIN raw SQL query), seat lock insert/release/extend/bulk; extend IRouteRepository for fuzzy city autocomplete using pg_trgm ILIKE
- [x] 4.4 Service: Implement SearchService (bus search with available seat count, seat map with single-query availability, city autocomplete, bus details); implement SeatLockService (create lock, bulk lock, extend lock, release lock, get active locks, release by journey, conflict check)
- [x] 4.5 Controller: Implement SearchController — GET /buses/search, GET /buses/{id}/seats, GET /buses/{id}, GET /cities/autocomplete, POST /seats/lock, POST /seats/lock/bulk, PUT /seats/lock/{id}/extend, GET /seats/lock/my-locks, DELETE /seats/lock/{id}, DELETE /seats/lock/by-seat/{seatId}
- [x] 4.6 Background Job: Implement PlatformCleanupJob as IHostedService using PeriodicTimer (60-second interval); expire seat locks via ISeatRepository.ExpireLocksAsync(); handle payment timeouts via IBookingRepository.HandlePaymentTimeoutsAsync()
- [x] 4.7 Angular Service: Implement SearchService (bus search, seat map, city autocomplete), SeatLockService (lock, release, countdown timer)
- [x] 4.8 Angular Component: Implement Home search page (autocomplete fields, date picker), Results page (bus cards), Seat map component (grid from seat_config, color coding, 10-min countdown timer, login modal trigger)

## Sprint 5 — Booking, Payment & Ticket Generation

- [x] 5.1 DB: Verify booking, booked_seat, payment, notification tables; confirm idx_booking_customer and idx_booking_bus indexes; confirm unique_seat_per_trip constraint
- [x] 5.2 EF Core Entity: Confirm Booking, BookedSeat, Payment, Notification, Coupon entities with all navigation properties
- [x] 5.3 Repository: Implement IBookingRepository / BookingRepository (create booking, create booked_seat rows, idempotency check, get by customer, get by id with seats); IPaymentRepository / PaymentRepository (create, update status); ICouponRepository / CouponRepository (validate, mark used)
- [x] 5.4 Service: Implement BookingService (booking creation with fare calculation, idempotency check, T&C version check, coupon validation and application, state machine transitions); implement PdfService using QuestPDF (generate ticket with all required fields); implement IEmailService / EmailService using MailKit + Channel<EmailMessage>
- [x] 5.5 Background Job: Implement EmailDispatchJob as IHostedService; drain Channel<EmailMessage>; call MailKit SMTP; write notification row with SENT/FAILED status
- [x] 5.6 Controller: Implement BookingController — POST /bookings, POST /bookings/{id}/pay, GET /bookings/mine, GET /bookings/{id}/ticket
- [x] 5.7 Angular Service: Implement BookingService (create booking, pay, get history, download ticket)
- [x] 5.8 Angular Component: Implement Booking summary page (passenger detail form, coupon field, fare breakdown), Dummy payment page, Confirmation page (PNR, download ticket button), My Bookings page (list with status chips, download ticket per booking)

## Sprint 6 — Cancellation, Refunds, Revenue & Dashboards

- [x] 6.1 DB: Verify cancellation and coupon tables; confirm cancellation FK to booking; confirm coupon FK to cancellation and customer
- [x] 6.2 EF Core Entity: Confirm Cancellation and Coupon entities; add navigation properties to Booking and Customer
- [x] 6.3 Repository: Implement ICancellationRepository / CancellationRepository (insert cancellation, update refund status); extend ICouponRepository (generate unique code, insert coupon, mark used); extend IBookingRepository (find future confirmed bookings by bus)
- [x] 6.4 Service: Implement CancellationService (customer cancellation with RefundCalculatorService for tiered 85%/50%/0% rules, operator cascade with atomicity, coupon generation, email enqueue); implement RefundCalculatorService (pure function: hours-until-departure → refund amount); implement CouponGenerator helper (unique code generation)
- [x] 6.5 Controller: Implement CancellationController — POST /bookings/{id}/cancel, POST /bookings/{id}/refund-complete; implement CouponController — POST /bookings/apply-coupon; add Admin revenue endpoints to AdminController — GET /admin/revenue, GET /admin/revenue/by-operator
- [x] 6.6 Angular Service: Implement CancellationService (cancel booking, calculate refund preview); extend AdminService (revenue endpoints); extend BookingService (apply coupon with live discount preview)
- [x] 6.7 Angular Component: Implement Cancel button on My Bookings (visible only for CONFIRMED bookings with departure > 12h, shows refund preview before confirm); implement Admin Revenue page (total revenue card, monthly chart, per-operator table); implement Operator Dashboard (full bookings table per bus, bus enable/disable toggle, boarding point editor); implement Coupon field on booking summary with live discount preview

## Sprint 7 — PNR Lookup, Security, Operating Days & Admin Enhancements

- [x] 7.1 DB: Add `bus_operating_days` table with columns: id, bus_id (FK → bus), day_of_week (INT 1–7), is_active, created_at, updated_at
- [x] 7.2 EF Core Entity: Add BusOperatingDay entity mapped to `bus_operating_days`; add navigation property on Bus
- [x] 7.3 Repository: Extend IBusRepository with CreateOperatingDaysAsync, GetOperatingDaysByBusAsync, UpdateOperatingDaysAsync; extend IBookingRepository with GetAllBookingsAsync (for PNR lookup) and HandlePaymentTimeoutsAsync
- [x] 7.4 Service: Implement PnrService (look up booking by 8-char PNR, privacy-safe response, canCancel logic); implement CaptchaService (generate 6-char alphanumeric challenge with sessionId, validate with expiry and attempt limits); implement DateValidationService (seat lock date range + same-day cutoff, booking date range); implement SeatLockSecurityService (IP/customer rate limiting, fraud detection, cleanup delegation)
- [x] 7.5 Controller: Implement PnrController — GET /captcha, POST /pnr/lookup; implement AdminSeatLockController — DELETE /admin/seats/lock/{id}; extend OperatorController with PUT /operator/buses/{busId}/operating-days, DELETE /operator/layouts/{id}, GET /operator/buses/{id}, DELETE /operator/buses/stops/{id}, PATCH /operator/buses/{id}/staff, GET /operator/profile; extend AdminController with GET /admin/operators, GET /admin/buses, GET /admin/operators/{operatorId}/buses, PATCH /admin/buses/{id}/toggle, GET /admin/tc, GET /tc/current, POST /admin/tc, GET /admin/audit-logs
- [x] 7.6 Background Job: Implement PlatformCleanupJob (replaces SeatLockExpiryJob) — PeriodicTimer 60s, expires seat locks via ISeatRepository.ExpireLocksAsync(), handles payment timeouts via IBookingRepository.HandlePaymentTimeoutsAsync(15 min); implement SeatLockCleanupService — BackgroundService every 5 min, delegates to ISeatLockSecurityService.CleanupExpiredLocksAsync()
- [x] 7.7 Middleware: Implement RateLimitingMiddleware — 30 req/min per IP, skip /health and /admin paths, support X-Forwarded-For and X-Real-IP headers
- [ ] 7.8 Angular Service: Implement PnrService (captcha fetch + PNR lookup); extend SeatLockService (bulk lock, extend lock, get my locks); extend OperatorService (operating days, layout delete, bus stop delete, staff update)
- [ ] 7.9 Angular Component: Implement PNR lookup page (captcha display + input, PNR form, result card with journey details and cancellation info); extend Seat Selection page (bulk lock support, extend lock button); extend Operator Bus Management (operating days editor, staff update form)
