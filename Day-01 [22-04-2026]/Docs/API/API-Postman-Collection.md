# API Postman Collection (Import Guide)

## Environment Variables

Create a Postman environment with:

- `baseUrl` = `http://localhost:5000`
- `customerEmail` = `john@example.com`
- `customerPassword` = `Pass@123`
- `operatorEmail` = `ops@skyline.com`
- `operatorPassword` = `Pass@123`
- `adminEmail` = `admin@example.com`
- `adminPassword` = `Pass@123`
- `busId` = `22222222-2222-2222-2222-222222222222`
- `bookingId` = `77777777-7777-7777-7777-777777777777`
- `routeId` = `44444444-4444-4444-4444-444444444444`
- `layoutId` = `55555555-5555-5555-5555-555555555555`
- `seatId` = `11111111-1111-1111-1111-111111111111`
- `lockId` = `66666666-6666-6666-6666-666666666666`
- `operatorId` = `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb`
- `stopId` = `88888888-8888-8888-8888-888888888888`

## Collection Request List

### Auth
- `POST {{baseUrl}}/auth/register/customer`
- `POST {{baseUrl}}/auth/register/operator`
- `POST {{baseUrl}}/auth/login`
- `POST {{baseUrl}}/auth/logout`
- `POST {{baseUrl}}/auth/accept-tc`
- `GET {{baseUrl}}/auth/me`

### Search
- `GET {{baseUrl}}/buses/search?from=Chennai&to=Bengaluru&date=2026-05-10`
- `GET {{baseUrl}}/buses/{{busId}}/seats?date=2026-05-10`
- `GET {{baseUrl}}/cities/autocomplete?q=che`

### Seat Lock
- `POST {{baseUrl}}/seats/lock`
- `DELETE {{baseUrl}}/seats/lock/{{lockId}}`

### Booking
- `POST {{baseUrl}}/bookings`
- `POST {{baseUrl}}/bookings/{{bookingId}}/pay`
- `GET {{baseUrl}}/bookings/mine`
- `GET {{baseUrl}}/bookings/{{bookingId}}/ticket`
- `POST {{baseUrl}}/bookings/apply-coupon`
- `POST {{baseUrl}}/bookings/{{bookingId}}/cancel`

### Admin
- `POST {{baseUrl}}/admin/routes`
- `GET {{baseUrl}}/routes`
- `PATCH {{baseUrl}}/admin/routes/{{routeId}}/toggle`
- `GET {{baseUrl}}/admin/operators/pending`
- `PATCH {{baseUrl}}/admin/operators/{{operatorId}}/approve`
- `PATCH {{baseUrl}}/admin/operators/{{operatorId}}/reject`
- `PATCH {{baseUrl}}/admin/operators/{{operatorId}}/toggle`
- `GET {{baseUrl}}/admin/buses/pending`
- `PATCH {{baseUrl}}/admin/buses/{{busId}}/approve`
- `PATCH {{baseUrl}}/admin/buses/{{busId}}/reject`
- `GET {{baseUrl}}/admin/revenue`
- `GET {{baseUrl}}/admin/revenue/by-operator`
- `POST {{baseUrl}}/bookings/{{bookingId}}/refund-complete`

### Operator
- `POST {{baseUrl}}/operator/layouts`
- `GET {{baseUrl}}/operator/layouts`
- `DELETE {{baseUrl}}/operator/layouts/{{layoutId}}`
- `POST {{baseUrl}}/operator/buses`
- `GET {{baseUrl}}/operator/buses`
- `GET {{baseUrl}}/operator/buses/{{busId}}`
- `GET {{baseUrl}}/operator/bookings`
- `PATCH {{baseUrl}}/operator/buses/{{busId}}/price`
- `PATCH {{baseUrl}}/operator/buses/{{busId}}/staff`
- `PATCH {{baseUrl}}/operator/buses/{{busId}}/disable`
- `DELETE {{baseUrl}}/operator/buses/{{busId}}`
- `POST {{baseUrl}}/operator/buses/{{busId}}/boarding-points`
- `POST {{baseUrl}}/operator/buses/{{busId}}/dropping-points`
- `DELETE {{baseUrl}}/operator/buses/stops/{{stopId}}`

## Body Samples

### Register Customer
```json
{
  "username": "john_doe",
  "email": "{{customerEmail}}",
  "password": "{{customerPassword}}"
}
```

### Login
```json
{
  "email": "{{customerEmail}}",
  "password": "{{customerPassword}}"
}
```

### Create Seat Lock
```json
{
  "seatId": "{{seatId}}",
  "busId": "{{busId}}",
  "journeyDate": "2026-05-10"
}
```

### Create Booking
```json
{
  "busId": "{{busId}}",
  "journeyDate": "2026-05-10",
  "passengers": [
    {
      "seatId": "{{seatId}}",
      "name": "John Doe",
      "age": 29,
      "gender": "Male"
    }
  ],
  "couponCode": "SAVE100"
}
```

### Apply Coupon
```json
{
  "couponCode": "SAVE100"
}
```

### Create Route
```json
{
  "sourceCity": "Chennai",
  "destinationCity": "Bengaluru"
}
```

### Create Layout
```json
{
  "layoutName": "2+2 Sleeper A",
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
```

### Register Bus
```json
{
  "routeId": "{{routeId}}",
  "layoutId": "{{layoutId}}",
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
