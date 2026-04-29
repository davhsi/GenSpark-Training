import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { BookingService } from '../../../core/services/booking.service';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './payment.component.html'
})
export class PaymentComponent implements OnInit {
  paymentForm: FormGroup;

  bookingId = '';
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private bookingService: BookingService
  ) {
    this.paymentForm = this.fb.group({
      cardNumber: ['', Validators.required],
      expiry: ['', Validators.required],
      cvv: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.bookingId = params['bookingId'] || '';
    });
  }

  get cardNumber() {
    return this.paymentForm.get('cardNumber')!;
  }

  get expiry() {
    return this.paymentForm.get('expiry')!;
  }

  get cvv() {
    return this.paymentForm.get('cvv')!;
  }

  onPay(): void {
    if (this.paymentForm.invalid) {
      this.paymentForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    this.bookingService.processPayment(this.bookingId).subscribe({
      next: booking => {
        this.isLoading = false;
        this.router.navigate(['/booking/confirmation'], {
          queryParams: { bookingId: booking.id, pnr: booking.pnr }
        });
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
        this.errorMessage =
          err.error?.message || 'Payment failed. Please try again.';
      }
    });
  }
}
