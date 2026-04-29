import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BookingDto,
  CreateBookingRequest
} from '../../shared/models/booking.models';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** Create a new booking for the given bus, date, and passengers. */
  createBooking(request: CreateBookingRequest): Observable<BookingDto> {
    return this.http.post<BookingDto>(`${this.apiUrl}/bookings`, request, {
      withCredentials: true
    });
  }

  /** Process payment for a pending booking. */
  processPayment(bookingId: string): Observable<BookingDto> {
    return this.http.post<BookingDto>(
      `${this.apiUrl}/bookings/${bookingId}/pay`,
      {},
      { withCredentials: true }
    );
  }

  /** Retrieve all bookings belonging to the authenticated customer. */
  getMyBookings(): Observable<BookingDto[]> {
    return this.http.get<BookingDto[]>(`${this.apiUrl}/bookings/mine`, {
      withCredentials: true
    });
  }

  /** Download the PDF ticket for a confirmed booking as a Blob. */
  downloadTicket(bookingId: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/bookings/${bookingId}/ticket`, {
      withCredentials: true,
      responseType: 'blob'
    });
  }

  /**
   * Apply a coupon code to a booking.
   * Returns the discount value, type, and code on success.
   */
  applyCoupon(couponCode: string): Observable<{ discountValue: number; discountType: string; code: string }> {
    return this.http.post<{ discountValue: number; discountType: string; code: string }>(
      `${this.apiUrl}/bookings/apply-coupon`,
      { couponCode },
      { withCredentials: true }
    );
  }
}
