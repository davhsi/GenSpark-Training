import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { PnrService, CaptchaResponse, PnrLookupResponse } from '../../../core/services/pnr.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-pnr-checker',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './pnr-checker.component.html',
  styleUrls: ['./pnr-checker.component.css']
})
export class PnrCheckerComponent implements OnInit {
  pnrForm: FormGroup;
  captchaData: CaptchaResponse | null = null;
  bookingDetails: PnrLookupResponse | null = null;
  isLoading = false;
  errorMessage = '';
  showResults = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private pnrService: PnrService,
    private authService: AuthService
  ) {
    this.pnrForm = this.fb.group({
      pnr: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(8)]],
      captchaInput: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadCaptcha();
  }

  get pnr() { return this.pnrForm.get('pnr')!; }
  get captchaInput() { return this.pnrForm.get('captchaInput')!; }

  loadCaptcha(): void {
    this.pnrService.getCaptcha().subscribe({
      next: (data: CaptchaResponse) => {
        this.captchaData = data;
        this.pnrForm.patchValue({ captchaInput: '' });
      },
      error: () => {
        this.errorMessage = 'Failed to load captcha. Please refresh the page.';
      }
    });
  }

  refreshCaptcha(): void {
    this.loadCaptcha();
  }

  onSubmit(): void {
    if (this.pnrForm.invalid || !this.captchaData) {
      this.pnrForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.showResults = false;

    const request = {
      pnr: this.pnr.value.toUpperCase(),
      captchaToken: `${this.captchaData.sessionId}:${this.captchaData.captchaText}`,
      captchaInput: this.captchaInput.value
    };

    this.pnrService.lookupPnr(request).subscribe({
      next: (booking: PnrLookupResponse) => {
        this.bookingDetails = booking;
        this.showResults = true;
        this.isLoading = false;
      },
      error: (error: any) => {
        this.isLoading = false;
        if (error.status === 404) {
          this.errorMessage = 'PNR not found. Please check your PNR and try again.';
        } else if (error.status === 400) {
          this.errorMessage = error.error?.message || 'Invalid captcha. Please try again.';
          this.loadCaptcha(); // Refresh captcha on validation error
        } else {
          this.errorMessage = 'An error occurred. Please try again later.';
        }
      }
    });
  }

  cancelBooking(): void {
    if (!this.bookingDetails?.canCancel) return;

    const user = this.authService.currentUser;
    if (user?.role === 'Customer') {
      // Already logged in — go straight to their bookings where they can cancel
      this.router.navigate(['/my-bookings']);
    } else {
      // Not logged in — send to login with a return URL
      this.router.navigate(['/login'], {
        queryParams: {
          returnUrl: '/my-bookings',
          message: 'Please login to cancel your booking'
        }
      });
    }
  }

  formatTime(time: string): string {
    if (!time) return '';
    const [hours, minutes] = time.split(':');
    return `${hours}:${minutes}`;
  }

  formatDate(date: string): string {
    if (!date) return '';
    const d = new Date(date);
    return d.toLocaleDateString('en-US', { 
      weekday: 'short', 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'confirmed': return 'bg-success';
      case 'cancelled': return 'bg-danger';
      case 'refunded': return 'bg-info';
      default: return 'bg-secondary';
    }
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'confirmed': return 'text-success';
      case 'cancelled': return 'text-danger';
      case 'refunded': return 'text-info';
      default: return 'text-secondary';
    }
  }
}
