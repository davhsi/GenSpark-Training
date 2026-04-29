# API Frontend Integration Checklist

## Base Setup

- [ ] Set API base URL per environment.
- [ ] Enable credentials (`withCredentials` / `credentials: include`) for auth-required requests.
- [ ] Use `Content-Type: application/json` for JSON bodies.
- [ ] Handle PDF response for ticket download.

## Auth and Session

- [ ] On login success, persist user profile (`role`, `email`, `userId`) in app state.
- [ ] On app init, call `GET /auth/me` to restore session.
- [ ] On `401`, clear auth state and redirect to login.
- [ ] On `403`, show access denied UI for route/module.
- [ ] Route users by role: `Admin`, `Operator`, `Customer`.

## Data Format Rules

- [ ] Send UUID values as strings.
- [ ] Send dates in `yyyy-MM-dd`.
- [ ] Send time strings as `HH:mm` for bus stop creation.
- [ ] Treat nullable fields safely (`null` checks in UI models).

## Search and Seat Flow

- [ ] Validate date before calling `/buses/search`.
- [ ] Refresh seat map before final booking submit.
- [ ] Handle lock conflict (`409`) by forcing seat reselection.
- [ ] Release seat lock on checkout cancel or page exit where applicable.

## Booking and Coupon

- [ ] Recalculate price when coupon apply succeeds.
- [ ] Show specific coupon errors from `400` responses.
- [ ] Handle `TC_REACCEPTANCE_REQUIRED` code before booking retry.
- [ ] Handle payment errors as retryable/non-retryable UI states.

## Admin and Operator

- [ ] Refresh queue lists after approve/reject/toggle actions.
- [ ] Optimistically update status where safe; rollback on failure.
- [ ] Ensure operator-specific screens never call admin endpoints.
- [ ] Validate required request fields before submit.

## Error Handling

- [ ] Use centralized API interceptor/service for `401`, `403`, `500`.
- [ ] Display backend `message` directly when user-friendly.
- [ ] Log raw error payload for debugging.
- [ ] Add fallback generic error toast.

## QA Scenarios

- [ ] Customer can register, login, search, lock seat, book, pay, download ticket.
- [ ] Customer can cancel booking and see refund status updates.
- [ ] Admin can approve/reject operators and buses.
- [ ] Operator can create layout, register bus, manage price/staff/stops.
- [ ] Expired/used/invalid coupon scenarios are verified.
