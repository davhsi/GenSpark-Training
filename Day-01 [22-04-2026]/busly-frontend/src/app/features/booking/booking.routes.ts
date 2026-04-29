import { Routes } from '@angular/router';
import { BookingSummaryComponent } from './booking-summary/booking-summary.component';
import { PaymentComponent } from './payment/payment.component';
import { ConfirmationComponent } from './confirmation/confirmation.component';

export const bookingRoutes: Routes = [
  { path: 'summary', component: BookingSummaryComponent },
  { path: 'payment', component: PaymentComponent },
  { path: 'confirmation', component: ConfirmationComponent }
];
