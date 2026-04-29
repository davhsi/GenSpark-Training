# API Status Code Matrix

## Auth

- `POST /auth/register/customer` -> `201`, `409`
- `POST /auth/register/operator` -> `201`, `409`
- `POST /auth/login` -> `200`, `401`
- `POST /auth/logout` -> `200`
- `POST /auth/accept-tc` -> `200`, `401`
- `GET /auth/me` -> `200`, `401`

## Search (Public)

- `GET /buses/search` -> `200`, `400`
- `GET /buses/{id}/seats` -> `200`, `400`
- `GET /cities/autocomplete` -> `200`

## Seat Lock (Customer)

- `POST /seats/lock` -> `201`, `401`, `409`
- `DELETE /seats/lock/{id}` -> `204`, `401`, `403`, `404`

## Booking (Customer)

- `POST /bookings` -> `201`, `401`, `403`, `400`
- `POST /bookings/{id}/pay` -> `200`, `401`, `403`, `400`
- `GET /bookings/mine` -> `200`, `401`
- `GET /bookings/{id}/ticket` -> `200`, `401`, `403`, `400`

## Coupon (Customer)

- `POST /bookings/apply-coupon` -> `200`, `401`, `400`

## Cancellation

- `POST /bookings/{id}/cancel` -> `200`, `401`, `403`, `404`, `400`
- `POST /bookings/{id}/refund-complete` -> `200`, `404`

## Admin

- `POST /admin/routes` -> `201`, `401`, `409`
- `GET /routes` -> `200`
- `PATCH /admin/routes/{id}/toggle` -> `200`, `401`
- `GET /admin/operators/pending` -> `200`, `401`
- `PATCH /admin/operators/{id}/approve` -> `200`, `401`
- `PATCH /admin/operators/{id}/reject` -> `200`, `401`
- `PATCH /admin/operators/{id}/toggle` -> `200`, `401`, `404`
- `GET /admin/buses/pending` -> `200`, `401`
- `PATCH /admin/buses/{id}/approve` -> `200`, `401`
- `PATCH /admin/buses/{id}/reject` -> `200`, `401`
- `GET /admin/revenue` -> `200`, `401`
- `GET /admin/revenue/by-operator` -> `200`, `401`

## Operator

- `POST /operator/layouts` -> `201`, `401`, `403`
- `GET /operator/layouts` -> `200`, `401`, `403`
- `DELETE /operator/layouts/{id}` -> `200`, `401`, `403`, `409`
- `POST /operator/buses` -> `201`, `401`, `403`
- `GET /operator/buses` -> `200`, `401`, `403`
- `GET /operator/buses/{id}` -> `200`, `401`, `403`, `404`
- `GET /operator/bookings` -> `200`, `401`, `403`
- `PATCH /operator/buses/{id}/price` -> `200`, `401`, `403`
- `PATCH /operator/buses/{id}/staff` -> `200`, `401`, `403`
- `PATCH /operator/buses/{id}/disable` -> `200`, `401`, `403`
- `DELETE /operator/buses/{id}` -> `200`, `401`, `403`
- `POST /operator/buses/{id}/boarding-points` -> `201`, `401`, `403`
- `POST /operator/buses/{id}/dropping-points` -> `201`, `401`, `403`
- `DELETE /operator/buses/stops/{id}` -> `200`, `401`, `403`
