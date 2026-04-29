import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuditLogDto, BusDto, ConvenienceFeeConfig, CreateRouteRequest, MonthlyRevenueDto, OperatorDto, OperatorRevenueDto, RouteDto, TcVersionDto, UpdateConvenienceFeeRequest } from '../../shared/models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // ─── Routes ────────────────────────────────────────────────────────────────

  /** Create a new route. POST /admin/routes */
  createRoute(request: CreateRouteRequest): Observable<RouteDto> {
    return this.http.post<RouteDto>(`${this.apiUrl}/admin/routes`, request, {
      withCredentials: true
    });
  }

  /** Get all active routes (public). GET /routes */
  getActiveRoutes(): Observable<RouteDto[]> {
    return this.http.get<RouteDto[]>(`${this.apiUrl}/routes`, {
      withCredentials: true
    });
  }

  /**
   * Get all routes (including inactive) for admin management. GET /admin/routes
   */
  getAllRoutes(): Observable<RouteDto[]> {
    return this.http.get<RouteDto[]>(`${this.apiUrl}/admin/routes`, {
      withCredentials: true
    });
  }

  /** Toggle a route's active status. PATCH /admin/routes/:id/toggle */
  toggleRoute(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/routes/${id}/toggle`, {}, {
      withCredentials: true
    });
  }

  // ─── Operators ─────────────────────────────────────────────────────────────

  /** Get all operators pending approval. GET /admin/operators/pending */
  getPendingOperators(): Observable<OperatorDto[]> {
    return this.http.get<OperatorDto[]>(`${this.apiUrl}/admin/operators/pending`, {
      withCredentials: true
    });
  }

  /** Get all operators for management. GET /admin/operators */
  getAllOperators(): Observable<OperatorDto[]> {
    return this.http.get<OperatorDto[]>(`${this.apiUrl}/admin/operators`, {
      withCredentials: true
    });
  }

  /** Approve a pending operator. PATCH /admin/operators/:id/approve */
  approveOperator(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/operators/${id}/approve`, {}, {
      withCredentials: true
    });
  }

  /** Reject a pending operator. PATCH /admin/operators/:id/reject */
  rejectOperator(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/operators/${id}/reject`, {}, {
      withCredentials: true
    });
  }

  /** Toggle an operator's active/suspended status. PATCH /admin/operators/:id/toggle */
  toggleOperator(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/operators/${id}/toggle`, {}, {
      withCredentials: true
    });
  }

  // ─── Buses ─────────────────────────────────────────────────────────────────

  /** Get all buses pending approval. GET /admin/buses/pending */
  getPendingBuses(): Observable<BusDto[]> {
    return this.http.get<BusDto[]>(`${this.apiUrl}/admin/buses/pending`, {
      withCredentials: true
    });
  }

  /** Get all buses for management. GET /admin/buses */
  getAllBuses(): Observable<BusDto[]> {
    return this.http.get<BusDto[]>(`${this.apiUrl}/admin/buses`, {
      withCredentials: true
    });
  }

  /** Get all buses by specific operator. GET /admin/operators/:id/buses */
  getBusesByOperator(operatorId: string): Observable<BusDto[]> {
    return this.http.get<BusDto[]>(`${this.apiUrl}/admin/operators/${operatorId}/buses`, {
      withCredentials: true
    });
  }

  /** Approve a pending bus. PATCH /admin/buses/:id/approve */
  approveBus(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/buses/${id}/approve`, {}, {
      withCredentials: true
    });
  }

  /** Reject a pending bus. PATCH /admin/buses/:id/reject */
  rejectBus(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/buses/${id}/reject`, {}, {
      withCredentials: true
    });
  }

  /** Toggle a bus's operational status. PATCH /admin/buses/:id/toggle */
  toggleBusStatus(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/buses/${id}/toggle`, {}, {
      withCredentials: true
    });
  }

  // ─── Revenue ───────────────────────────────────────────────────────────────

  /** Get monthly platform revenue. GET /admin/revenue */
  getRevenue(): Observable<MonthlyRevenueDto[]> {
    return this.http.get<MonthlyRevenueDto[]>(`${this.apiUrl}/admin/revenue`, {
      withCredentials: true
    });
  }

  /** Get revenue broken down by operator. GET /admin/revenue/by-operator */
  getRevenueByOperator(): Observable<OperatorRevenueDto[]> {
    return this.http.get<OperatorRevenueDto[]>(`${this.apiUrl}/admin/revenue/by-operator`, {
      withCredentials: true
    });
  }

  /** Get system audit logs. GET /admin/audit-logs */
  getAuditLogs(): Observable<AuditLogDto[]> {
    return this.http.get<AuditLogDto[]>(`${this.apiUrl}/admin/audit-logs`, {
      withCredentials: true
    });
  }

  /** Get all T&C versions. GET /admin/tc */
  getAllTcVersions(): Observable<TcVersionDto[]> {
    return this.http.get<TcVersionDto[]>(`${this.apiUrl}/admin/tc`, {
      withCredentials: true
    });
  }

  /** Publish new T&C version. POST /admin/tc */
  publishTc(version: string, content: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/admin/tc`, { version, content }, {
      withCredentials: true
    });
  }

  // ─── Platform Config ───────────────────────────────────────────────────────

  /** Get current convenience fee config. GET /admin/config/convenience-fee */
  getConvenienceFeeConfig(): Observable<ConvenienceFeeConfig> {
    return this.http.get<ConvenienceFeeConfig>(`${this.apiUrl}/admin/config/convenience-fee`, {
      withCredentials: true
    });
  }

  /** Update convenience fee config. PATCH /admin/config/convenience-fee */
  updateConvenienceFeeConfig(request: UpdateConvenienceFeeRequest): Observable<any> {
    return this.http.patch(`${this.apiUrl}/admin/config/convenience-fee`, request, {
      withCredentials: true
    });
  }
}
