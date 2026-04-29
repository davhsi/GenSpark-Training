import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Route guard that allows access only to authenticated users with the Admin role.
 * Redirects to /login if the check fails.
 */
export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.currentUser?.role === 'Admin') {
    return true;
  }

  router.navigate(['/login']);
  return false;
};
