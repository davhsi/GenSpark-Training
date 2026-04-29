# Busly API Request Samples

## Auth

### `POST /auth/register/customer`
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "Pass@123"
}
```

### `POST /auth/register/operator`
```json
{
  "companyName": "Skyline Travels",
  "email": "ops@skyline.com",
  "password": "Pass@123",
  "phone": "+91-9876543210"
}
```

### `POST /auth/login`
```json
{
  "email": "john@example.com",
  "password": "Pass@123"
}
```

### `POST /auth/logout`
No request body.

### `POST /auth/accept-tc`
No request body.

### `GET /auth/me`
No request body.

---

## Public Search

### `GET /buses/search?from=Chennai&to=Bengaluru&date=2026-05-10`
No request body.

### `GET /buses/{busId}/seats?date=2026-05-10`
No request body.

### `GET /cities/autocomplete?q=che`
No request body.

---

## Seat Lock (Customer)

### `POST /seats/lock`
```json
{
  "seatId": "11111111-1111-1111-1111-111111111111",
  "busId": "22222222-2222-2222-2222-222222222222",
  "journeyDate": "2026-05-10"
}
```

### `DELETE /seats/lock/{lockId}`
No request body.

---

## Booking (Customer)

### `POST /bookings`
```json
{
  "busId": "22222222-2222-2222-2222-222222222222",
  "journeyDate": "2026-05-10",
  "passengers": [
    {
      "seatId": "11111111-1111-1111-1111-111111111111",
      "name": "John Doe",
      "age": 29,
      "gender": "Male"
    },
    {
      "seatId": "33333333-3333-3333-3333-333333333333",
      "name": "Jane Doe",
      "age": 27,
      "gender": "Female"
    }
  ],
  "couponCode": "SAVE100"
}
```

### `POST /bookings/{bookingId}/pay`
No request body.

### `GET /bookings/mine`
No request body.

### `GET /bookings/{bookingId}/ticket`
No request body.

---

## Coupon (Customer)

### `POST /bookings/apply-coupon`
```json
{
  "couponCode": "SAVE100"
}
```

---

## Cancellation

### `POST /bookings/{bookingId}/cancel`
No request body.

### `POST /bookings/{bookingId}/refund-complete` (Admin)
No request body.

---

## Admin

### `POST /admin/routes`
```json
{
  "sourceCity": "Chennai",
  "destinationCity": "Bengaluru"
}
```

### `GET /routes`
No request body.

### `PATCH /admin/routes/{routeId}/toggle`
No request body.

### `GET /admin/operators/pending`
No request body.

### `PATCH /admin/operators/{operatorId}/approve`
No request body.

### `PATCH /admin/operators/{operatorId}/reject`
No request body.

### `PATCH /admin/operators/{operatorId}/toggle`
No request body.

### `GET /admin/buses/pending`
No request body.

### `PATCH /admin/buses/{busId}/approve`
No request body.

### `PATCH /admin/buses/{busId}/reject`
No request body.

### `GET /admin/revenue`
No request body.

### `GET /admin/revenue/by-operator`
No request body.

---

## Operator

### `POST /operator/layouts`
```json
{
  "layoutName": "2+2 Sleeper A",
  "seatConfig": {
    "decks": [
      {
        "name": "LOWER",
        "rows": 6,
        "cols": 4
      },
      {
        "name": "UPPER",
        "rows": 6,
        "cols": 4
      }
    ]
  }
}
```

### `GET /operator/layouts`
No request body.

### `DELETE /operator/layouts/{layoutId}`
No request body.

### `POST /operator/buses`
```json
{
  "routeId": "44444444-4444-4444-4444-444444444444",
  "layoutId": "55555555-5555-5555-5555-555555555555",
  "busNumber": "TN-01-AB-1234",
  "busName": "Skyline Express",
  "ownerName": "Skyline Travels Pvt Ltd",
  "driverName": "Ravi Kumar",
  "driverPhone": "+91-9000000001",
  "conductorName": "Suresh",
  "conductorPhone": "+91-9000000002",
  "basePrice": 950.0
}
```

### `GET /operator/buses`
No request body.

### `GET /operator/buses/{busId}`
No request body.

### `GET /operator/bookings`
No request body.

### `PATCH /operator/buses/{busId}/price`
```json
{
  "basePrice": 999.0
}
```

### `PATCH /operator/buses/{busId}/staff`
```json
{
  "driverName": "Arun",
  "driverPhone": "+91-9000000010",
  "conductorName": "Karthik",
  "conductorPhone": "+91-9000000011"
}
```

### `PATCH /operator/buses/{busId}/disable`
No request body.

### `DELETE /operator/buses/{busId}`
No request body.

### `POST /operator/buses/{busId}/boarding-points`
```json
{
  "city": "Chennai",
  "address": "Koyambedu Bus Terminus",
  "scheduledTime": "21:00"
}
```

### `POST /operator/buses/{busId}/dropping-points`
```json
{
  "city": "Bengaluru",
  "address": "Madiwala Junction",
  "scheduledTime": "05:30"
}
```

### `DELETE /operator/buses/stops/{stopId}`
No request body.
