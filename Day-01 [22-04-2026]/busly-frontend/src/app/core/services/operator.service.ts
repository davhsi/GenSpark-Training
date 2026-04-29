import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AddBusStopRequest,
  BusDetailDto,
  CreateLayoutRequest,
  LayoutDto,
  OperatorProfileDto,
  RegisterBusRequest,
  UpdateOperatingDaysRequest,
  UpdatePriceRequest
} from '../../shared/models/operator.models';

@Injectable({
  providedIn: 'root'
})
export class OperatorService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** Create a new seat layout. */
  createLayout(request: CreateLayoutRequest): Observable<LayoutDto> {
    return this.http.post<LayoutDto>(`${this.apiUrl}/operator/layouts`, request, {
      withCredentials: true
    });
  }

  /** Retrieve all layouts belonging to the authenticated operator. */
  getLayouts(): Observable<LayoutDto[]> {
    return this.http.get<LayoutDto[]>(`${this.apiUrl}/operator/layouts`, {
      withCredentials: true
    });
  }

  /** Delete a layout by ID. */
  deleteLayout(layoutId: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/operator/layouts/${layoutId}`, {
      withCredentials: true
    });
  }

  /** Register a new bus on a route with a given layout. */
  registerBus(request: RegisterBusRequest): Observable<BusDetailDto> {
    return this.http.post<BusDetailDto>(`${this.apiUrl}/operator/buses`, request, {
      withCredentials: true
    });
  }

  /** Add a boarding point to a bus. */
  addBoardingPoint(busId: string, request: AddBusStopRequest): Observable<any> {
    return this.http.post<any>(
      `${this.apiUrl}/operator/buses/${busId}/boarding-points`,
      request,
      { withCredentials: true }
    );
  }

  /** Add a dropping point to a bus. */
  addDroppingPoint(busId: string, request: AddBusStopRequest): Observable<any> {
    return this.http.post<any>(
      `${this.apiUrl}/operator/buses/${busId}/dropping-points`,
      request,
      { withCredentials: true }
    );
  }

  /** Remove a boarding or dropping point. */
  removeBusStop(stopId: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/operator/buses/stops/${stopId}`, {
      withCredentials: true
    });
  }

  /** Update the base price for a bus. */
  updateBusPrice(busId: string, request: UpdatePriceRequest): Observable<any> {
    return this.http.patch<any>(
      `${this.apiUrl}/operator/buses/${busId}/price`,
      request,
      { withCredentials: true }
    );
  }

  /** Update driver and conductor info. */
  updateBusStaff(busId: string, request: any): Observable<any> {
    return this.http.patch<any>(
      `${this.apiUrl}/operator/buses/${busId}/staff`,
      request,
      { withCredentials: true }
    );
  }

  /** Disable a bus (take it off sale). */
  disableBus(busId: string): Observable<any> {
    return this.http.patch<any>(
      `${this.apiUrl}/operator/buses/${busId}/disable`,
      {},
      { withCredentials: true }
    );
  }

  /** Permanently remove a bus. */
  removeBus(busId: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/operator/buses/${busId}`, {
      withCredentials: true
    });
  }

  /** Retrieve all buses belonging to the authenticated operator. */
  getBuses(): Observable<BusDetailDto[]> {
    return this.http.get<BusDetailDto[]>(`${this.apiUrl}/operator/buses`, {
      withCredentials: true
    });
  }

  /** Retrieve a single bus by ID (includes its stops). */
  getBusById(busId: string): Observable<BusDetailDto> {
    return this.http.get<BusDetailDto>(`${this.apiUrl}/operator/buses/${busId}`, {
      withCredentials: true
    });
  }

  /** Retrieve all bookings for the authenticated operator's buses. */
  getBookings(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/operator/bookings`, {
      withCredentials: true
    });
  }

  /** Update operating days for a bus. */
  updateOperatingDays(busId: string, request: UpdateOperatingDaysRequest): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/operator/buses/${busId}/operating-days`, request, {
      withCredentials: true
    });
  }

  /** Get the operator's profile including approval status. */
  getProfile(): Observable<OperatorProfileDto> {
    return this.http.get<OperatorProfileDto>(`${this.apiUrl}/operator/profile`, {
      withCredentials: true
    });
  }
}
