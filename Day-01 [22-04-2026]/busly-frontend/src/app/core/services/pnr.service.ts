import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CaptchaResponse {
  sessionId: string;
  captchaText: string;
  message: string;
}

export interface PnrLookupRequest {
  pnr: string;
  captchaToken: string;
  captchaInput: string;
}

export interface PnrLookupResponse {
  pnr: string;
  status: string;
  customerName: string;
  customerEmail: string;
  journeyDate: string;
  fromCity: string;
  toCity: string;
  busNumber: string;
  departureTime: string;
  arrivalTime: string;
  seatNumbers: string[];
  totalAmount: number;
  bookedAt: string;
  canCancel: boolean;
  cancellationReason?: string;
  refundAmount?: number;
  refundStatus?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PnrService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getCaptcha(): Observable<CaptchaResponse> {
    return this.http.get<CaptchaResponse>(`${this.apiUrl}/captcha`);
  }

  lookupPnr(request: PnrLookupRequest): Observable<PnrLookupResponse> {
    return this.http.post<PnrLookupResponse>(`${this.apiUrl}/pnr/lookup`, request);
  }
}
