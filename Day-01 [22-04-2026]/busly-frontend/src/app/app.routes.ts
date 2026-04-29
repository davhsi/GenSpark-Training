import { Routes } from '@angular/router';
import { adminGuard } from './core/guards/admin.guard';
import { operatorGuard } from './core/guards/operator.guard';
import { customerGuard } from './core/guards/customer.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/search', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'search',
    loadChildren: () =>
      import('./features/search/search.routes').then(m => m.searchRoutes)
  },
  {
    path: 'booking',
    canActivate: [customerGuard],
    loadChildren: () =>
      import('./features/booking/booking.routes').then(m => m.bookingRoutes)
  },
  {
    path: 'my-bookings',
    canActivate: [customerGuard],
    loadChildren: () =>
      import('./features/my-bookings/my-bookings.routes').then(m => m.myBookingsRoutes)
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadChildren: () =>
      import('./features/admin/admin.routes').then(m => m.adminRoutes)
  },
  {
    path: 'operator',
    canActivate: [operatorGuard],
    loadChildren: () =>
      import('./features/operator/operator.routes').then(m => m.operatorRoutes)
  },
  {
    path: 'pnr-check',
    loadComponent: () =>
      import('./features/pnr-checker/pnr-checker/pnr-checker.component').then(m => m.PnrCheckerComponent)
  }
];
