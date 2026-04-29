# API Error Codes

## Standard Error Shape

```json
{
  "message": "Human readable error message"
}
```

Some flows may return:

```json
{
  "code": "MACHINE_READABLE_CODE"
}
```

---

## Error Catalog

### `AUTH_INVALID_CREDENTIALS`
- **HTTP:** `401 Unauthorized`
- **Sample:**
```json
{
  "message": "Invalid credentials"
}
```
- **Where:** `POST /auth/login`
- **Client action:** Show login error, allow retry.

### `AUTH_INVALID_TOKEN`
- **HTTP:** `401 Unauthorized`
- **Sample:**
```json
{
  "message": "Invalid token"
}
```
- **Where:** Protected endpoints when token/claims are invalid
- **Client action:** Clear user session and redirect to login.

### `AUTH_ACCESS_DENIED`
- **HTTP:** `403 Forbidden`
- **Sample:**
```json
{
  "message": "Access denied"
}
```
- **Where:** Cancellation and role-protected flows
- **Client action:** Show permission error page/snackbar.

### `AUTH_ROLE_FORBIDDEN`
- **HTTP:** `403 Forbidden`
- **Sample:**
```json
{
  "message": "Forbidden"
}
```
- **Where:** Accessing endpoints outside user role
- **Client action:** Route user to allowed module.

### `OPERATOR_NOT_APPROVED`
- **HTTP:** `403 Forbidden`
- **Sample:**
```json
{
  "message": "Operator account is not approved."
}
```
- **Where:** Any `/operator/*` management endpoint when operator status is not `APPROVED`
- **Client action:** Show account status message and ask operator to contact admin.

### `AUTH_TC_REACCEPTANCE_REQUIRED`
- **HTTP:** `403 Forbidden`
- **Sample:**
```json
{
  "code": "TC_REACCEPTANCE_REQUIRED"
}
```
- **Where:** `POST /bookings`
- **Client action:** Force T&C re-accept flow, then retry.

### `REGISTRATION_EMAIL_EXISTS`
- **HTTP:** `409 Conflict`
- **Sample:**
```json
{
  "message": "Email already registered"
}
```
- **Where:** Register customer/operator
- **Client action:** Ask user to login or use another email.

### `ROUTE_ALREADY_EXISTS`
- **HTTP:** `409 Conflict`
- **Sample:**
```json
{
  "message": "Route already exists"
}
```
- **Where:** `POST /admin/routes`
- **Client action:** Prevent duplicate route creation in UI.

### `OPERATOR_NOT_FOUND`
- **HTTP:** `404 Not Found`
- **Sample:**
```json
{
  "message": "Operator not found"
}
```
- **Where:** `PATCH /admin/operators/{id}/toggle`
- **Client action:** Refresh queue and remove stale row.

### `BOOKING_NOT_FOUND`
- **HTTP:** `404 Not Found`
- **Sample:**
```json
{
  "message": "Booking not found"
}
```
- **Where:** `POST /bookings/{id}/cancel`
- **Client action:** Refresh bookings list.

### `CANCELLATION_NOT_FOUND`
- **HTTP:** `404 Not Found`
- **Sample:**
```json
{
  "message": "Cancellation not found"
}
```
- **Where:** `POST /bookings/{id}/refund-complete`
- **Client action:** Refresh admin cancellation/refund view.

### `SEAT_ALREADY_LOCKED_OR_BOOKED`
- **HTTP:** `409 Conflict`
- **Sample:**
```json
{
  "message": "Seat is already locked or booked"
}
```
- **Where:** `POST /seats/lock`
- **Client action:** Mark seat unavailable and prompt reselection.

### `SEAT_LOCK_NOT_FOUND`
- **HTTP:** `404 Not Found`
- **Sample:**
```json
{
  "message": "Seat lock not found"
}
```
- **Where:** `DELETE /seats/lock/{id}`
- **Client action:** Ignore if UI already updated.

### `SEAT_LOCK_OWNERSHIP_ERROR`
- **HTTP:** `403 Forbidden`
- **Sample:**
```json
{
  "message": "Forbidden"
}
```
- **Where:** `DELETE /seats/lock/{id}`
- **Client action:** Reload seat map.

### `COUPON_NOT_FOUND`
- **HTTP:** `400 Bad Request`
- **Sample:**
```json
{
  "message": "Coupon not found"
}
```
- **Where:** `POST /bookings/apply-coupon`
- **Client action:** Show invalid coupon message.

### `COUPON_EXPIRED`
- **HTTP:** `400 Bad Request`
- **Sample:**
```json
{
  "message": "Coupon has expired"
}
```
- **Where:** `POST /bookings/apply-coupon`
- **Client action:** Ask user to remove coupon.

### `COUPON_ALREADY_USED`
- **HTTP:** `400 Bad Request`
- **Sample:**
```json
{
  "message": "Coupon has already been used"
}
```
- **Where:** `POST /bookings/apply-coupon`
- **Client action:** Ask user to try another coupon.

### `COUPON_NOT_ISSUED_TO_USER`
- **HTTP:** `400 Bad Request`
- **Sample:**
```json
{
  "message": "Coupon is not issued to this customer"
}
```
- **Where:** `POST /bookings/apply-coupon`
- **Client action:** Reject coupon in checkout UI.

### `BOOKING_INVALID_COUPON`
- **HTTP:** `400 Bad Request`
- **Sample:**
```json
{
  "message": "Invalid coupon"
}
```
- **Where:** `POST /bookings`
- **Client action:** Remove coupon and recalculate fare.

### `INVALID_DATE_FORMAT`
- **HTTP:** `400 Bad Request`
- **Sample:**
```json
{
  "message": "Invalid date format. Use yyyy-MM-dd."
}
```
- **Where:** Search and seat map APIs
- **Client action:** Fix client date formatter.

### `GENERIC_BAD_REQUEST`
- **HTTP:** `400 Bad Request`
- **Sample:**
```json
{
  "message": "Business rule validation failed"
}
```
- **Where:** Pricing, payment, cancellation, etc.
- **Client action:** Surface message and keep form state.
