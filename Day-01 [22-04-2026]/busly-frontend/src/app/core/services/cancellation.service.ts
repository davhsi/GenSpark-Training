import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CancellationDto } from '../../shared/models/booking.models';

@Injectable({
  providedIn: 'root'
})
export class CancellationService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** Cancel a booking by ID. POST /bookings/:bookingId/cancel */
  cancelBooking(bookingId: string): Observable<CancellationDto> {
    return this.http.post<CancellationDto>(
      `${this.apiUrl}/bookings/${bookingId}/cancel`,
      {},
      { withCredentials: true }
    );
  }

  /**
   * Pure client-side refund preview calculation.
   * >24h before departure → 85% of baseFare
   * 12–24h before departure → 50% of baseFare
   * <12h before departure → 0
   */
  calculateRefundPreview(
    journeyDate: string,
    departureTime: string,
    baseFare: number
  ): number {
    const [hours, minutes] = departureTime.split(':').map(Number);
    const departure = new Date(`${journeyDate}T${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:00`);
    const now = new Date();
    const hoursUntilDeparture = (departure.getTime() - now.getTime()) / (1000 * 60 * 60);

    if (hoursUntilDeparture > 24) {
      return Math.round(baseFare * 0.85 * 100) / 100;
    } else if (hoursUntilDeparture >= 12) {
      return Math.round(baseFare * 0.5 * 100) / 100;
    } else {
      return 0;
    }
  }

  /**
   * Calculate refund percentage based on time until departure
   * >24h before departure → 85%
   * 12–24h before departure → 50%
   * <12h before departure → 0%
   */
  calculateRefundPercentage(
    journeyDate: string,
    departureTime: string
  ): number {
    const [hours, minutes] = departureTime.split(':').map(Number);
    const departure = new Date(`${journeyDate}T${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:00`);
    const now = new Date();
    const hoursUntilDeparture = (departure.getTime() - now.getTime()) / (1000 * 60 * 60);

    if (hoursUntilDeparture > 24) {
      return 85;
    } else if (hoursUntilDeparture >= 12) {
      return 50;
    } else {
      return 0;
    }
  }
}
