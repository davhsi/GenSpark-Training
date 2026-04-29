import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateSeatLockRequest,
  SeatLockDto
} from '../../shared/models/search.models';

@Injectable({
  providedIn: 'root'
})
export class SeatLockService {
  private readonly apiUrl = environment.apiUrl;

  private _activeLocks: Map<string, SeatLockDto> = new Map();
  private _countdown$ = new BehaviorSubject<number>(0);
  private _countdownInterval: ReturnType<typeof setInterval> | null = null;

  /** Observable stream of seconds remaining on the active seat lock. */
  public countdown$ = this._countdown$.asObservable();

  /** The currently active seat locks, mapped by seatId. */
  get activeLocks(): Map<string, SeatLockDto> {
    return this._activeLocks;
  }

  /** The primary active seat lock, or null if none. */
  get activeLock(): SeatLockDto | null {
    return this._activeLocks.size > 0 ? Array.from(this._activeLocks.values())[0] : null;
  }

  /** Get the latest expiry time across all active locks. */
  get latestExpiryTime(): Date | null {
    if (this._activeLocks.size === 0) return null;
    
    const expiryTimes = Array.from(this._activeLocks.values())
      .map(lock => lock.expiresAt)
      .filter((expiry): expiry is string => expiry !== null && expiry !== undefined);
    
    return expiryTimes.length > 0 ? new Date(Math.max(...expiryTimes.map(d => new Date(d).getTime()))) : null;
  }

  constructor(private http: HttpClient) {}

  /**
   * Lock a seat for the current user.
   * POST /seats/lock
   * On success, stores the lock and starts the countdown timer.
   */
  lockSeat(request: CreateSeatLockRequest): Observable<SeatLockDto> {
    return this.http
      .post<SeatLockDto>(`${this.apiUrl}/seats/lock`, request, {
        withCredentials: true
      })
      .pipe(
        tap((lock) => {
          this._activeLocks.set(lock.seatId, lock);
          this.startCountdown();
        })
      );
  }

  /**
   * Release an active seat lock.
   * DELETE /seats/lock/:lockId
   * On success, removes the specific lock and updates timer if needed.
   */
  releaseLock(lockId: string): Observable<any> {
    return this.http
      .delete(`${this.apiUrl}/seats/lock/${lockId}`, {
        withCredentials: true
      })
      .pipe(
        tap(() => {
          this.removeLock(lockId);
        })
      );
  }

  /**
   * Remove a specific lock from the active locks and update countdown if needed.
   */
  private removeLock(lockId: string): void {
    this._activeLocks.delete(lockId);
    
    if (this._activeLocks.size === 0) {
      this.clearLock();
    } else {
      // Restart countdown with remaining locks
      this.startCountdown();
    }
  }

  /**
   * Release lock by seat ID (finds the corresponding lock ID).
   */
  releaseLockBySeatId(seatId: string): Observable<any> {
    const lock = this._activeLocks.get(seatId);
    if (!lock) {
      // If no lock found in memory, still try to release by seat ID
      return this.http.delete(`${this.apiUrl}/seats/lock/by-seat/${seatId}`, {
        withCredentials: true
      });
    }
    
    return this.releaseLock(lock.lockId);
  }

  /**
   * Clear the active lock, stop the countdown timer, and emit 0.
   */
  clearLock(): void {
    this._activeLocks.clear();
    this.stopTimer();
    this._countdown$.next(0);
  }

  /**
   * Start the countdown timer based on the latest expiry time across all locks.
   */
  private startCountdown(): void {
    this.stopTimer();

    const latestExpiry = this.latestExpiryTime;
    if (!latestExpiry) {
      return;
    }

    const expiresAt = latestExpiry.getTime();
    const updateCountdown = () => {
      const secondsRemaining = Math.max(
        0,
        Math.floor((expiresAt - Date.now()) / 1000)
      );
      this._countdown$.next(secondsRemaining);

      if (secondsRemaining <= 0) {
        this.clearLock();
      }
    };

    // Emit the initial value immediately
    updateCountdown();

    this._countdownInterval = setInterval(updateCountdown, 1000);
  }

  /**
   * Clear the active countdown interval.
   */
  private stopTimer(): void {
    if (this._countdownInterval !== null) {
      clearInterval(this._countdownInterval);
      this._countdownInterval = null;
    }
  }
}
