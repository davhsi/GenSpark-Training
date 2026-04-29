import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { BookingService } from '../../../core/services/booking.service';

@Component({
  selector: 'app-confirmation',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './confirmation.component.html'
})
export class ConfirmationComponent implements OnInit {
  bookingId = '';
  pnr = '';

  isDownloading = false;
  downloadError: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private bookingService: BookingService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.bookingId = params['bookingId'] || '';
      this.pnr = params['pnr'] || '';
    });
  }

  downloadTicket(): void {
    if (!this.bookingId) return;

    this.isDownloading = true;
    this.downloadError = null;

    this.bookingService.downloadTicket(this.bookingId).subscribe({
      next: (blob: Blob) => {
        this.isDownloading = false;
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `ticket-${this.pnr || this.bookingId}.pdf`;
        anchor.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.isDownloading = false;
        this.downloadError = 'Failed to download ticket. Please try again.';
      }
    });
  }
}
