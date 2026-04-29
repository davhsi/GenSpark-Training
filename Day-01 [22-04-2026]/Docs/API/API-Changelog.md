# API Changelog

## v1.1.0 - 2026-04-24

### Added
- Operator bookings endpoint now returns real booking data:
  - `GET /operator/bookings`

### Changed
- Operator management endpoints now enforce operator account status = `APPROVED`.
- Operator endpoints can return:
  - `403 Forbidden` with message: `Operator account is not approved.`
- Booking creation response sample corrected to reflect initial booking status:
  - `status = INITIATED` on `POST /bookings`

## v1.0.0 - 2026-04-24

### Added
- Auth endpoints:
  - `POST /auth/register/customer`
  - `POST /auth/register/operator`
  - `POST /auth/login`
  - `POST /auth/logout`
  - `POST /auth/accept-tc`
  - `GET /auth/me`
- Public search endpoints:
  - `GET /buses/search`
  - `GET /buses/{id}/seats`
  - `GET /cities/autocomplete`
- Seat lock endpoints:
  - `POST /seats/lock`
  - `DELETE /seats/lock/{id}`
- Booking endpoints:
  - `POST /bookings`
  - `POST /bookings/{id}/pay`
  - `GET /bookings/mine`
  - `GET /bookings/{id}/ticket`
- Coupon endpoint:
  - `POST /bookings/apply-coupon`
- Cancellation endpoints:
  - `POST /bookings/{id}/cancel`
  - `POST /bookings/{id}/refund-complete`
- Admin endpoints:
  - `POST /admin/routes`
  - `GET /routes`
  - `PATCH /admin/routes/{id}/toggle`
  - `GET /admin/operators/pending`
  - `PATCH /admin/operators/{id}/approve`
  - `PATCH /admin/operators/{id}/reject`
  - `PATCH /admin/operators/{id}/toggle`
  - `GET /admin/buses/pending`
  - `PATCH /admin/buses/{id}/approve`
  - `PATCH /admin/buses/{id}/reject`
  - `GET /admin/revenue`
  - `GET /admin/revenue/by-operator`
- Operator endpoints:
  - `POST /operator/layouts`
  - `GET /operator/layouts`
  - `DELETE /operator/layouts/{id}`
  - `POST /operator/buses`
  - `GET /operator/buses`
  - `GET /operator/buses/{id}`
  - `PATCH /operator/buses/{id}/price`
  - `PATCH /operator/buses/{id}/staff`
  - `PATCH /operator/buses/{id}/disable`
  - `DELETE /operator/buses/{id}`
  - `POST /operator/buses/{id}/boarding-points`
  - `POST /operator/buses/{id}/dropping-points`
  - `DELETE /operator/buses/stops/{id}`

### Notes
- JWT is sent via HttpOnly cookie (`busly_token`).
- Role-based authorization is enforced by policies (`Admin`, `Operator`, `Customer`).
- Main error formats are `{ "message": "..." }` and `{ "code": "..." }`.
