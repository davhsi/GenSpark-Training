import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BookingService } from '../../../core/services/booking.service';
import { CancellationService } from '../../../core/services/cancellation.service';
import { AuthService } from '../../../core/services/auth.service';
import { BookingDto } from '../../../shared/models/booking.models';
import { CancelBookingDialogComponent, CancelBookingDialogData } from '../../../shared/components/cancel-booking-dialog/cancel-booking-dialog.component';

@Component({
  selector: 'app-booking-list',
  standalone: true,
  imports: [CommonModule, RouterModule, CancelBookingDialogComponent],
  templateUrl: './booking-list.component.html'
})
export class BookingListComponent implements OnInit {
  bookings: BookingDto[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  downloadingId: string | null = null;
  downloadError: string | null = null;

  cancellingId: string | null = null;
  cancelError: string | null = null;
  refundPreview: { [bookingId: string]: number } = {};
  showCancelDialog: boolean = false;
  cancelDialogData: CancelBookingDialogData = { pnr: '', refundAmount: 0, refundPercentage: 0 };
  currentBookingToCancel: BookingDto | null = null;

  constructor(
    private bookingService: BookingService,
    private cancellationService: CancellationService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  private loadBookings(): void {
    this.isLoading = true;
    this.bookingService.getMyBookings().subscribe({
      next: bookings => {
        this.bookings = bookings;
        this.isLoading = false;
        // Pre-calculate refund previews for all cancellable bookings
        bookings.forEach(b => {
          if (this.canCancel(b)) {
            this.refundPreview[b.id] = this.getRefundPreview(b);
          }
        });
      },
      error: () => {
        this.errorMessage = 'Failed to load bookings. Please try again.';
        this.isLoading = false;
      }
    });
  }

  /** Returns true if the booking can be cancelled (status is CONFIRMED and > 12h before departure). */
  canCancel(booking: BookingDto): boolean {
    if (booking.status !== 'CONFIRMED') return false;
    
    // Use actual departure time if available, fallback to 08:00
    const departureTimeString = booking.departureTime || '08:00';
    const [hours, minutes] = departureTimeString.split(':').map(Number);
    const departure = new Date(`${booking.journeyDate}T${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:00`);
    const now = new Date();
    const hoursUntilDeparture = (departure.getTime() - now.getTime()) / (1000 * 60 * 60);

    return hoursUntilDeparture > 12;
  }

  /** Calculates estimated refund using actual departure time or 08:00 fallback. */
  getRefundPreview(booking: BookingDto): number {
    const departureTimeString = booking.departureTime || '08:00';
    return this.cancellationService.calculateRefundPreview(
      booking.journeyDate,
      departureTimeString,
      booking.baseFare ?? 0
    );
  }

  cancelBooking(booking: BookingDto): void {
    // Check if user is authenticated before allowing cancellation
    if (!this.authService.isLoggedIn) {
      this.cancelError = 'You must be logged in to cancel a booking. Please log in and try again.';
      return;
    }

    const refund = this.refundPreview[booking.id] ?? this.getRefundPreview(booking);
    const percentage = this.cancellationService.calculateRefundPercentage(
      booking.journeyDate,
      booking.departureTime || '08:00'
    );
    
    this.currentBookingToCancel = booking;
    this.cancelDialogData = {
      pnr: booking.pnr,
      refundAmount: refund,
      refundPercentage: percentage
    };
    this.showCancelDialog = true;
  }

  onCancelDialog(): void {
    this.showCancelDialog = false;
    this.currentBookingToCancel = null;
  }

  onConfirmCancel(): void {
    if (!this.currentBookingToCancel) return;
    
    this.showCancelDialog = false;
    this.cancellingId = this.currentBookingToCancel.id;
    this.cancelError = null;

    this.cancellationService.cancelBooking(this.currentBookingToCancel.id).subscribe({
      next: () => {
        this.cancellingId = null;
        this.currentBookingToCancel = null;
        this.loadBookings();
      },
      error: () => {
        this.cancellingId = null;
        this.cancelError = `Failed to cancel booking ${this.currentBookingToCancel?.pnr}. Please try again.`;
        this.currentBookingToCancel = null;
      }
    });
  }

  /** Returns a Bootstrap badge class based on booking status. */
  statusBadgeClass(status: string | undefined): string {
    switch (status) {
      case 'INITIATED':
        return 'bg-secondary';
      case 'PAYMENT_PENDING':
        return 'bg-warning text-dark';
      case 'CONFIRMED':
        return 'bg-success';
      case 'CANCELLED':
        return 'bg-danger';
      case 'REFUNDED':
        return 'bg-info text-dark';
      default:
        return 'bg-secondary';
    }
  }

  downloadTicket(booking: BookingDto): void {
    if (booking.status !== 'CONFIRMED') return;

    this.downloadingId = booking.id;
    this.downloadError = null;

    this.bookingService.downloadTicket(booking.id).subscribe({
      next: (blob: Blob) => {
        this.downloadingId = null;
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `ticket-${booking.pnr || booking.id}.pdf`;
        anchor.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.downloadingId = null;
        this.downloadError = `Failed to download ticket for booking ${booking.pnr}.`;
      }
    });
  }
}
