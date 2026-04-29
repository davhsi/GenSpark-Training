import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Route guard that allows access only to authenticated users with the Customer role.
 * Redirects to /login if the check fails.
 */
export const customerGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.currentUser?.role === 'Customer') {
    return true;
  }

  router.navigate(['/login']);
  return false;
};
