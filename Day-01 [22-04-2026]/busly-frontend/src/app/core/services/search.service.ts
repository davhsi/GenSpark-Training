import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BusSearchResultDto,
  SeatMapResponse
} from '../../shared/models/search.models';

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Search for buses matching the given route and date.
   * GET /buses/search?from=&to=&date=
   */
  searchBuses(from: string, to: string, date: string): Observable<BusSearchResultDto[]> {
    return this.http.get<BusSearchResultDto[]>(`${this.apiUrl}/buses/search`, {
      params: { from, to, date },
      withCredentials: true
    });
  }

  /**
   * Retrieve the seat map for a specific bus on a given date.
   * GET /buses/:busId/seats?date=
   */
  getSeatMap(busId: string, date: string): Observable<SeatMapResponse> {
    return this.http.get<SeatMapResponse>(`${this.apiUrl}/buses/${busId}/seats`, {
      params: { date },
      withCredentials: true
    });
  }

  /**
   * Fetch city name suggestions for autocomplete.
   * GET /cities/autocomplete?q=
   */
  getCitySuggestions(q: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/cities/autocomplete`, {
      params: { q },
      withCredentials: true
    });
  }

  /**
   * Get details for a specific bus.
   * GET /buses/:busId
   */
  getBusDetails(busId: string): Observable<BusSearchResultDto> {
    return this.http.get<BusSearchResultDto>(`${this.apiUrl}/buses/${busId}`, {
      withCredentials: true
    });
  }

  /**
   * Get the current convenience fee config (public).
   * GET /config/convenience-fee
   */
  getConvenienceFeeConfig(): Observable<{ feeType: string; feeValue: number }> {
    return this.http.get<{ feeType: string; feeValue: number }>(`${this.apiUrl}/config/convenience-fee`);
  }
}
