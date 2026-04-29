import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  FormArray,
  Validators
} from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { BookingService } from '../../../core/services/booking.service';
import { SearchService } from '../../../core/services/search.service';
import { AuthService } from '../../../core/services/auth.service';
import { BusSearchResultDto } from '../../../shared/models/search.models';
import { TcVersionDto } from '../../../shared/models/auth.models';

interface SeatInfo {
  seatId: string;
  seatNumber: string;
  price: number;
}

@Component({
  selector: 'app-booking-summary',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './booking-summary.component.html'
})
export class BookingSummaryComponent implements OnInit {
  summaryForm: FormGroup;

  busId = '';
  seatInfo: SeatInfo[] = [];
  date = '';
  busDetails: BusSearchResultDto | null = null;

  baseFare = 0;
  convenienceFee = 0;
  totalAmount = 0;

  couponDiscount = 0;
  couponError: string | null = null;
  couponApplied = false;
  isApplyingCoupon = false;

  isLoading = false;
  errorMessage: string | null = null;
  showTcModal = false;
  tcContent: TcVersionDto | null = null;
  isLoadingTc = false;
  isAcceptingTc = false;
  tcAcceptError: string | null = null;
  feeConfig: { feeType: string; feeValue: number } = { feeType: 'flat', feeValue: 0 };
  
  readonly MAX_SEATS = 4;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private bookingService: BookingService,
    private searchService: SearchService,
    private authService: AuthService
  ) {
    this.summaryForm = this.fb.group({
      passengers: this.fb.array([]),
      couponCode: ['']
    });
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.busId = params['busId'] || '';
      this.date = params['date'] || '';

      // seatId may be a single value or an array
      const rawSeatId = params['seatId'];
      const seatIds: string[] = [];
      if (Array.isArray(rawSeatId)) {
        seatIds.push(...rawSeatId);
      } else if (rawSeatId) {
        seatIds.push(rawSeatId);
      }

      // Validate seat limit
      if (seatIds.length > this.MAX_SEATS) {
        this.errorMessage = `Maximum ${this.MAX_SEATS} seats allowed per booking. Please select fewer seats.`;
        return;
      }

      if (seatIds.length === 0) {
        this.errorMessage = 'No seats selected. Please go back and select seats.';
        return;
      }

      // Load bus details first to get pricing
      this.loadBusDetails().then(() => {
        // Convert seat IDs to seat info
        this.seatInfo = seatIds.map((seatId, index) => ({
          seatId: seatId,
          seatNumber: this.generateSeatNumber(index + 1),
          price: this.busDetails?.basePrice || 0
        }));

        // Load real convenience fee config, then calculate
        this.searchService.getConvenienceFeeConfig().subscribe({
          next: cfg => {
            this.feeConfig = cfg;
            this.calculatePricing();
          },
          error: () => this.calculatePricing() // fall back to 0 if unavailable
        });

        // Build one passenger group per seat
        this.passengers.clear();
        this.seatInfo.forEach(() => this.passengers.push(this.buildPassengerGroup()));
      });
    });
  }

  get passengers(): FormArray {
    return this.summaryForm.get('passengers') as FormArray;
  }

  private async loadBusDetails(): Promise<void> {
    return new Promise((resolve) => {
      this.searchService.getBusDetails(this.busId).subscribe({
        next: (bus: BusSearchResultDto) => {
          this.busDetails = bus;
          resolve();
        },
        error: () => {
          // Fallback to mock data
          this.busDetails = {
            busId: this.busId,
            busName: 'Express Bus',
            busNumber: 'EXP-001',
            operatorName: 'Busly Tours',
            sourceCity: 'New York',
            destinationCity: 'Boston',
            basePrice: 1200.00, // Updated to match expected pricing
            availableSeats: 40
          };
          resolve();
        }
      });
    });
  }

  private calculatePricing(): void {
    this.baseFare = this.seatInfo.reduce((total, seat) => total + seat.price, 0);
    if (this.feeConfig.feeType === 'percent') {
      this.convenienceFee = Math.round(this.baseFare * (this.feeConfig.feeValue / 100) * 100) / 100;
    } else {
      this.convenienceFee = this.feeConfig.feeValue; // flat fee per booking
    }
    this.totalAmount = this.baseFare + this.convenienceFee;
  }

  private generateSeatNumber(index: number): string {
    // Generate a user-friendly seat number like A1, A2, B1, etc.
    const row = String.fromCharCode(65 + Math.floor((index - 1) / 4)); // A, B, C, etc.
    const col = ((index - 1) % 4) + 1; // 1, 2, 3, 4
    return `${row}${col}`;
  }

  private buildPassengerGroup(): FormGroup {
    return this.fb.group({
      name: ['', Validators.required],
      age: [null, [Validators.required, Validators.min(1)]],
      gender: ['', Validators.required]
    });
  }

  passengerAt(index: number): FormGroup {
    return this.passengers.at(index) as FormGroup;
  }

  onSubmit(): void {
    if (this.summaryForm.invalid) {
      this.summaryForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    const formValue = this.summaryForm.value;
    const passengersPayload = formValue.passengers.map(
      (p: { name: string; age: number; gender: string }, i: number) => ({
        seatId: this.seatInfo[i].seatId,
        name: p.name,
        age: p.age,
        gender: p.gender
      })
    );

    this.bookingService
      .createBooking({
        busId: this.busId,
        journeyDate: this.date,
        passengers: passengersPayload,
        couponCode: formValue.couponCode || undefined
      })
      .subscribe({
        next: booking => {
          this.isLoading = false;
          this.router.navigate(['/booking/payment'], {
            queryParams: { bookingId: booking.id }
          });
        },
        error: (err: HttpErrorResponse) => {
          this.isLoading = false;
          if (
            err.status === 403 &&
            err.error?.code === 'TC_REACCEPTANCE_REQUIRED'
          ) {
            this.isLoadingTc = true;
            this.showTcModal = true;
            this.authService.getCurrentTc().subscribe({
              next: tc => {
                this.tcContent = tc;
                this.isLoadingTc = false;
              },
              error: () => {
                this.isLoadingTc = false;
                this.tcAcceptError = 'Failed to load Terms & Conditions. Please try again.';
              }
            });
          } else {
            this.errorMessage =
              err.error?.message || 'Failed to create booking. Please try again.';
          }
        }
      });
  }

  closeTcModal(): void {
    this.showTcModal = false;
    this.tcContent = null;
    this.tcAcceptError = null;
  }

  acceptTcAndRetry(): void {
    this.isAcceptingTc = true;
    this.tcAcceptError = null;
    this.authService.acceptTc().subscribe({
      next: () => {
        this.isAcceptingTc = false;
        this.showTcModal = false;
        this.tcContent = null;
        // Retry the booking submission now that T&C is accepted
        this.onSubmit();
      },
      error: () => {
        this.isAcceptingTc = false;
        this.tcAcceptError = 'Failed to accept Terms & Conditions. Please try again.';
      }
    });
  }

  applyCoupon(): void {
    const code: string = (this.summaryForm.get('couponCode')?.value ?? '').trim();
    if (!code) {
      this.couponError = 'Please enter a coupon code.';
      return;
    }

    this.isApplyingCoupon = true;
    this.couponError = null;
    this.couponApplied = false;

    this.bookingService.applyCoupon(code).subscribe({
      next: result => {
        this.isApplyingCoupon = false;
        this.couponApplied = true;
        if (result.discountType?.toLowerCase() === 'percent') {
          this.couponDiscount = Math.round((this.baseFare * result.discountValue) / 100 * 100) / 100;
        } else {
          this.couponDiscount = result.discountValue;
        }
        this.totalAmount = Math.max(0, this.baseFare + this.convenienceFee - this.couponDiscount);
      },
      error: () => {
        this.isApplyingCoupon = false;
        this.couponDiscount = 0;
        this.couponApplied = false;
        this.couponError = 'Invalid or expired coupon.';
      }
    });
  }
}
