import { HttpInterceptorFn } from '@angular/common/http';

/**
 * Functional HTTP interceptor that sets withCredentials: true on every
 * outgoing request so the browser automatically includes the HttpOnly
 * JWT cookie. Angular never reads or attaches the token manually.
 */
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authReq = req.clone({ withCredentials: true });
  return next(authReq);
};
