# API Auth Guide

## Authentication Model

- Authentication uses JWT in an HttpOnly cookie named `busly_token`.
- Frontend does not read token from JavaScript.
- Browser sends cookie automatically when credentials are enabled.

## Login Flow

### 1) Request
`POST /auth/login`

```json
{
  "email": "john@example.com",
  "password": "Pass@123"
}
```

### 2) Response
- HTTP `200 OK`
- Sets cookie: `busly_token`
- Returns:

```json
{
  "role": "Customer",
  "email": "john@example.com",
  "userId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
}
```

## Logout Flow

`POST /auth/logout`

- HTTP `200 OK`
- Server deletes `busly_token` cookie.

## Session Check

`GET /auth/me`

- Use this after page refresh/app bootstrap.
- If valid cookie exists, returns current user profile.
- If invalid/missing cookie, returns `401`.

## Frontend Request Settings

Use credentials in all API requests that need auth.

### Fetch
```js
fetch(url, {
  method: "GET",
  credentials: "include"
});
```

### Angular HttpClient
```ts
this.http.get(url, { withCredentials: true });
```

## Role-Based Access

- `Admin` APIs: `/admin/*` and admin refund completion.
- `Operator` APIs: `/operator/*`.
- `Customer` APIs: `/bookings*`, `/seats/lock*`, coupon apply, cancellation.
- Operator management APIs also require the operator account status to be `APPROVED`; otherwise API returns `403`.

If role does not match policy -> `403 Forbidden`.

## Common Auth Failures

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

### T&C Reacceptance Check (Booking)
```json
{
  "code": "TC_REACCEPTANCE_REQUIRED"
}
```

## Recommended Client Handling

- On `401`: clear client auth state, redirect to login.
- On `403`: keep session, show permission/access message.
- On `TC_REACCEPTANCE_REQUIRED`: open T&C consent flow, call `/auth/accept-tc`, retry booking.
