import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { LoginCredentials, User } from '../models/user.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly loginUrl = 'https://dummyjson.com/auth/login';

  private userSubject = new BehaviorSubject<User | null>(null);

  // Public observable — Header, Dashboard subscribe to this
  user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient) {}

  login(credentials: LoginCredentials) {
    return this.http.post<User>(this.loginUrl, credentials).pipe(
      tap((user) => {
        sessionStorage.setItem('token', user.accessToken || '');
        this.userSubject.next(user);
      }),
      catchError((error) => {
        console.error('Login failed', error);
        return throwError(() => error);
      })
    );
  }

  logout() {
    sessionStorage.removeItem('token');
    this.userSubject.next(null);
  }

  isLoggedIn() {
    return !!sessionStorage.getItem('token');
  }

  getCurrentUser() {
    return this.userSubject.value;
  }
}
