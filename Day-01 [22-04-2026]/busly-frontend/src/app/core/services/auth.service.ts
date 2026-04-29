import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CurrentUser,
  LoginRequest,
  RegisterCustomerRequest,
  RegisterOperatorRequest,
  TcStatusDto,
  TcVersionDto,
  UserProfile
} from '../../shared/models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = environment.apiUrl;

  // JWT is stored in an HttpOnly cookie managed by the browser.
  // Angular never reads or stores the token — it is invisible to JavaScript.
  private _currentUser$ = new BehaviorSubject<CurrentUser | null>(null);

  /** Observable stream of the currently authenticated user. */
  public currentUser$ = this._currentUser$.asObservable();

  constructor(private http: HttpClient) {}

  /** Returns the current user snapshot, or null if not authenticated. */
  get currentUser(): CurrentUser | null {
    return this._currentUser$.value;
  }

  /** Returns true if a user is currently authenticated. */
  get isLoggedIn(): boolean {
    return this._currentUser$.value !== null;
  }

  /** Register a new customer account. */
  registerCustomer(request: RegisterCustomerRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/register/customer`, request, {
      withCredentials: true
    });
  }

  /** Register a new bus operator account. */
  registerOperator(request: RegisterOperatorRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/register/operator`, request, {
      withCredentials: true
    });
  }

  /**
   * Authenticate with email and password.
   * The server sets an HttpOnly cookie — Angular receives only role/email/userId.
   * On success, emits the current user to all subscribers.
   */
  login(request: LoginRequest): Observable<{ role: string; email: string; userId: string }> {
    return this.http
      .post<{ role: string; email: string; userId: string }>(
        `${this.apiUrl}/auth/login`,
        request,
        { withCredentials: true }
      )
      .pipe(
        tap((response) => {
          const currentUser: CurrentUser = {
            userId: response.userId,
            email: response.email,
            role: response.role as CurrentUser['role'],
            token: '' // token is in HttpOnly cookie — not accessible here
          };
          this._currentUser$.next(currentUser);
        })
      );
  }

  /**
   * Call GET /auth/me to restore the current user from the HttpOnly cookie.
   * Use this on app initialisation to re-hydrate the session after a page refresh.
   */
  restoreSession(): Observable<UserProfile> {
    return this.http
      .get<UserProfile>(`${this.apiUrl}/auth/me`, { withCredentials: true })
      .pipe(
        tap((profile) => {
          const currentUser: CurrentUser = {
            userId: profile.userId,
            email: profile.email,
            role: profile.role as CurrentUser['role'],
            token: ''
          };
          this._currentUser$.next(currentUser);
        })
      );
  }

  /**
   * Log out — calls the server to clear the HttpOnly cookie,
   * then clears the local user state.
   */
  logout(): Observable<any> {
    return this.http
      .post(`${this.apiUrl}/auth/logout`, {}, { withCredentials: true })
      .pipe(tap(() => this._currentUser$.next(null)));
  }

  /** Accept the current active Terms & Conditions for the authenticated user. */
  acceptTc(): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/accept-tc`, {}, {
      withCredentials: true
    });
  }

  /** Get the user's T&C acceptance status. */
  getTcStatus(): Observable<TcStatusDto> {
    return this.http.get<TcStatusDto>(`${this.apiUrl}/auth/tc-status`, {
      withCredentials: true
    });
  }

  /** Get the current active T&C content. */
  getCurrentTc(): Observable<TcVersionDto> {
    return this.http.get<TcVersionDto>(`${this.apiUrl}/tc/current`);
  }

  /** Fetch the authenticated user's profile from the API. */
  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/auth/me`, {
      withCredentials: true
    });
  }
}
