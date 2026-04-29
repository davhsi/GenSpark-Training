# Busly API Response Samples

## Auth

### `POST /auth/register/customer` - `201`
```json
{
  "message": "Customer registered successfully",
  "userId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
}
```

### `POST /auth/register/operator` - `201`
```json
{
  "message": "Operator registered. Awaiting admin approval.",
  "userId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
}
```

### `POST /auth/login` - `200`
```json
{
  "role": "Customer",
  "email": "john@example.com",
  "userId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
}
```

### `POST /auth/logout` - `200`
```json
{
  "message": "Logged out successfully"
}
```

### `POST /auth/accept-tc` - `200`
```json
{
  "message": "T&C accepted successfully"
}
```

### `GET /auth/me` - `200`
```json
{
  "userId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "email": "john@example.com",
  "role": "Customer",
  "name": "John Doe",
  "tcAccepted": true,
  "tcVersion": "v1.0"
}
```

---

## Public Search

### `GET /buses/search` - `200`
```json
[
  {
    "busId": "22222222-2222-2222-2222-222222222222",
    "busName": "Skyline Express",
    "busNumber": "TN-01-AB-1234",
    "operatorName": "Skyline Travels",
    "sourceCity": "Chennai",
    "destinationCity": "Bengaluru",
    "basePrice": 950.0,
    "availableSeats": 22
  }
]
```

### `GET /buses/{busId}/seats` - `200`
```json
{
  "layoutConfig": "{\"decks\":[{\"name\":\"LOWER\",\"rows\":6,\"cols\":4}]}",
  "seatStatuses": [
    {
      "seatId": "11111111-1111-1111-1111-111111111111",
      "seatNumber": 1,
      "seatType": "SEATER",
      "deck": "LOWER",
      "status": "AVAILABLE",
      "passengerGender": null
    },
    {
      "seatId": "33333333-3333-3333-3333-333333333333",
      "seatNumber": 2,
      "seatType": "SEATER",
      "deck": "LOWER",
      "status": "BOOKED",
      "passengerGender": "Female"
    }
  ]
}
```

### `GET /cities/autocomplete` - `200`
```json
[
  "Chennai",
  "Chengalpattu",
  "Cheyyar"
]
```

---

## Seat Lock (Customer)

### `POST /seats/lock` - `201`
```json
{
  "lockId": "66666666-6666-6666-6666-666666666666",
  "seatId": "11111111-1111-1111-1111-111111111111",
  "busId": "22222222-2222-2222-2222-222222222222",
  "journeyDate": "2026-05-10",
  "expiresAt": "2026-05-10T14:10:00Z"
}
```

### `DELETE /seats/lock/{lockId}` - `204`
No response body.

---

## Booking (Customer)

### `POST /bookings` - `201`
```json
{
  "id": "77777777-7777-7777-7777-777777777777",
  "pnr": "77777777",
  "busId": "22222222-2222-2222-2222-222222222222",
  "journeyDate": "2026-05-10",
  "baseFare": 1900.0,
  "convenienceFee": 50.0,
  "totalAmount": 1850.0,
  "status": "INITIATED",
  "bookedAt": "2026-05-01T12:10:00Z",
  "seats": [
    {
      "seatId": "11111111-1111-1111-1111-111111111111",
      "passengerName": "John Doe",
      "passengerAge": 29,
      "passengerGender": "Male"
    },
    {
      "seatId": "33333333-3333-3333-3333-333333333333",
      "passengerName": "Jane Doe",
      "passengerAge": 27,
      "passengerGender": "Female"
    }
  ]
}
```

### `POST /bookings/{bookingId}/pay` - `200`
```json
{
  "id": "77777777-7777-7777-7777-777777777777",
  "pnr": "77777777",
  "busId": "22222222-2222-2222-2222-222222222222",
  "journeyDate": "2026-05-10",
  "baseFare": 1900.0,
  "convenienceFee": 50.0,
  "totalAmount": 1850.0,
  "status": "CONFIRMED",
  "bookedAt": "2026-05-01T12:10:00Z",
  "seats": [
    {
      "seatId": "11111111-1111-1111-1111-111111111111",
      "passengerName": "John Doe",
      "passengerAge": 29,
      "passengerGender": "Male"
    }
  ]
}
```

### `GET /bookings/mine` - `200`
```json
[
  {
    "id": "77777777-7777-7777-7777-777777777777",
    "pnr": "77777777",
    "busId": "22222222-2222-2222-2222-222222222222",
    "journeyDate": "2026-05-10",
    "baseFare": 1900.0,
    "convenienceFee": 50.0,
    "totalAmount": 1850.0,
    "status": "CONFIRMED",
    "bookedAt": "2026-05-01T12:10:00Z",
    "seats": [
      {
        "seatId": "11111111-1111-1111-1111-111111111111",
        "passengerName": "John Doe",
        "passengerAge": 29,
        "passengerGender": "Male"
      }
    ]
  }
]
```

### `GET /bookings/{bookingId}/ticket` - `200`
Binary PDF response (`Content-Type: application/pdf`).

---

## Coupon (Customer)

### `POST /bookings/apply-coupon` - `200`
```json
{
  "discountValue": 100.0,
  "discountType": "flat",
  "code": "SAVE100"
}
```

---

## Cancellation

### `POST /bookings/{bookingId}/cancel` - `200`
```json
{
  "bookingId": "77777777-7777-7777-7777-777777777777",
  "refundAmount": 1650.0,
  "refundStatus": "PENDING"
}
```

### `POST /bookings/{bookingId}/refund-complete` - `200`
```json
{
  "message": "Refund processed"
}
```

---

## Admin

### `POST /admin/routes` - `201`
```json
{
  "id": "44444444-4444-4444-4444-444444444444",
  "sourceCity": "Chennai",
  "destinationCity": "Bengaluru",
  "isActive": true
}
```

### `GET /routes` - `200`
```json
[
  {
    "id": "44444444-4444-4444-4444-444444444444",
    "sourceCity": "Chennai",
    "destinationCity": "Bengaluru",
    "isActive": true
  }
]
```

### `PATCH /admin/routes/{routeId}/toggle` - `200`
```json
{
  "message": "Route toggled"
}
```

### `GET /admin/operators/pending` - `200`
```json
[
  {
    "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "companyName": "Skyline Travels",
    "email": "ops@skyline.com",
    "phone": "+91-9876543210",
    "status": "PENDING",
    "createdAt": "2026-05-01T10:00:00Z"
  }
]
```

### `PATCH /admin/operators/{operatorId}/approve` - `200`
```json
{
  "message": "Operator approved"
}
```

### `PATCH /admin/operators/{operatorId}/reject` - `200`
```json
{
  "message": "Operator rejected"
}
```

### `PATCH /admin/operators/{operatorId}/toggle` - `200`
```json
{
  "message": "Operator status toggled"
}
```

### `GET /admin/buses/pending` - `200`
```json
[
  {
    "id": "22222222-2222-2222-2222-222222222222",
    "busNumber": "TN-01-AB-1234",
    "busName": "Skyline Express",
    "ownerName": "Skyline Travels Pvt Ltd",
    "status": "PENDING",
    "operatorId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "routeId": "44444444-4444-4444-4444-444444444444",
    "basePrice": 950.0,
    "driverName": "Ravi Kumar",
    "driverPhone": "+91-9000000001",
    "conductorName": "Suresh",
    "conductorPhone": "+91-9000000002",
    "sourceCity": "Chennai",
    "destinationCity": "Bengaluru",
    "layoutName": "2+2 Sleeper A",
    "createdAt": "2026-05-01T11:00:00Z"
  }
]
```

### `PATCH /admin/buses/{busId}/approve` - `200`
```json
{
  "message": "Bus approved"
}
```

### `PATCH /admin/buses/{busId}/reject` - `200`
```json
{
  "message": "Bus rejected"
}
```

### `GET /admin/revenue` - `200`
```json
[
  {
    "year": 2026,
    "month": 5,
    "totalConvenienceFee": 24500.0
  }
]
```

### `GET /admin/revenue/by-operator` - `200`
```json
[
  {
    "operatorName": "Skyline Travels",
    "bookingCount": 320,
    "totalBaseFare": 285000.0,
    "totalConvenienceFee": 9600.0
  }
]
```

---

## Operator

### `POST /operator/layouts` - `201`
```json
{
  "id": "55555555-5555-5555-5555-555555555555",
  "layoutName": "2+2 Sleeper A",
  "totalSeats": 48,
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

### `GET /operator/layouts` - `200`
```json
[
  {
    "id": "55555555-5555-5555-5555-555555555555",
    "layoutName": "2+2 Sleeper A",
    "totalSeats": 48,
    "seatConfig": {
      "decks": [
        {
          "name": "LOWER",
          "rows": 6,
          "cols": 4
        }
      ]
    }
  }
]
```

### `DELETE /operator/layouts/{layoutId}` - `200`
```json
{
  "message": "Layout removed"
}
```

### `POST /operator/buses` - `201`
```json
{
  "id": "22222222-2222-2222-2222-222222222222",
  "busNumber": "TN-01-AB-1234",
  "busName": "Skyline Express",
  "ownerName": "Skyline Travels Pvt Ltd",
  "status": "PENDING",
  "basePrice": 950.0,
  "driverName": "Ravi Kumar",
  "driverPhone": "+91-9000000001",
  "conductorName": "Suresh",
  "conductorPhone": "+91-9000000002",
  "routeId": "44444444-4444-4444-4444-444444444444",
  "sourceCity": "Chennai",
  "destinationCity": "Bengaluru",
  "layoutId": "55555555-5555-5555-5555-555555555555",
  "createdAt": "2026-05-01T11:00:00Z",
  "stops": []
}
```

### `GET /operator/buses` - `200`
```json
[
  {
    "id": "22222222-2222-2222-2222-222222222222",
    "busNumber": "TN-01-AB-1234",
    "busName": "Skyline Express",
    "ownerName": "Skyline Travels Pvt Ltd",
    "status": "ACTIVE",
    "basePrice": 950.0,
    "driverName": "Ravi Kumar",
    "driverPhone": "+91-9000000001",
    "conductorName": "Suresh",
    "conductorPhone": "+91-9000000002",
    "routeId": "44444444-4444-4444-4444-444444444444",
    "sourceCity": "Chennai",
    "destinationCity": "Bengaluru",
    "layoutId": "55555555-5555-5555-5555-555555555555",
    "createdAt": "2026-05-01T11:00:00Z",
    "stops": [
      {
        "id": "88888888-8888-8888-8888-888888888888",
        "type": "BOARDING",
        "city": "Chennai",
        "address": "Koyambedu Bus Terminus",
        "scheduledTime": "21:00:00"
      }
    ]
  }
]
```

### `GET /operator/buses/{busId}` - `200`
```json
{
  "id": "22222222-2222-2222-2222-222222222222",
  "busNumber": "TN-01-AB-1234",
  "busName": "Skyline Express",
  "ownerName": "Skyline Travels Pvt Ltd",
  "status": "ACTIVE",
  "basePrice": 950.0,
  "driverName": "Ravi Kumar",
  "driverPhone": "+91-9000000001",
  "conductorName": "Suresh",
  "conductorPhone": "+91-9000000002",
  "routeId": "44444444-4444-4444-4444-444444444444",
  "sourceCity": "Chennai",
  "destinationCity": "Bengaluru",
  "layoutId": "55555555-5555-5555-5555-555555555555",
  "createdAt": "2026-05-01T11:00:00Z",
  "stops": []
}
```

### `GET /operator/bookings` - `200`
```json
[
  {
    "id": "77777777-7777-7777-7777-777777777777",
    "pnr": "77777777",
    "busId": "22222222-2222-2222-2222-222222222222",
    "journeyDate": "2026-05-10",
    "baseFare": 1900.0,
    "convenienceFee": 50.0,
    "totalAmount": 1850.0,
    "status": "CONFIRMED",
    "bookedAt": "2026-05-01T12:10:00Z",
    "seats": [
      {
        "seatId": "11111111-1111-1111-1111-111111111111",
        "passengerName": "John Doe",
        "passengerAge": 29,
        "passengerGender": "Male"
      }
    ]
  }
]
```

### `PATCH /operator/buses/{busId}/price` - `200`
```json
{
  "message": "Price updated"
}
```

### `PATCH /operator/buses/{busId}/staff` - `200`
```json
{
  "message": "Staff updated"
}
```

### `PATCH /operator/buses/{busId}/disable` - `200`
```json
{
  "message": "Bus disabled"
}
```

### `DELETE /operator/buses/{busId}` - `200`
```json
{
  "message": "Bus removed"
}
```

### `POST /operator/buses/{busId}/boarding-points` - `201`
```json
{
  "message": "Boarding point added"
}
```

### `POST /operator/buses/{busId}/dropping-points` - `201`
```json
{
  "message": "Dropping point added"
}
```

### `DELETE /operator/buses/stops/{stopId}` - `200`
```json
{
  "message": "Stop removed"
}
```

---

## Common Error Samples

### `400 Bad Request`
```json
{
  "message": "Invalid coupon"
}
```

### `401 Unauthorized`
```json
{
  "message": "Invalid token"
}
```

### `403 Forbidden`
```json
{
  "message": "Operator account is not approved."
}
```

### `404 Not Found`
```json
{
  "message": "Booking not found"
}
```

### `409 Conflict`
```json
{
  "message": "Email already registered"
}
```
