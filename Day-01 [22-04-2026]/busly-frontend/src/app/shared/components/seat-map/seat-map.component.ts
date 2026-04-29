import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, Subscription } from 'rxjs';
import { SearchService } from '../../../core/services/search.service';
import { SeatLockService } from '../../../core/services/seat-lock.service';
import { AuthService } from '../../../core/services/auth.service';
import { SeatAvailabilityDto } from '../../models/search.models';

interface SeatRow {
  rowIndex: number;
  seats: (SeatAvailabilityDto | null)[];  // null = aisle gap
}

interface DeckLayout {
  deckName: string;
  rows: SeatRow[];
}

@Component({
  selector: 'app-seat-map',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './seat-map.component.html',
  styleUrls: ['./seat-map.component.css']
})
export class SeatMapComponent implements OnInit, OnDestroy {
  @Input() busId: string = '';
  @Input() date: string = '';
  @Output() seatSelected = new EventEmitter<any>();
  @Output() seatDeselected = new EventEmitter<string>();

  decks: DeckLayout[] = [];
  selectedSeatIds: Set<string> = new Set();
  countdown$!: Observable<number>;

  isLoading = false;
  errorMessage: string | null = null;
  showLoginModal = false;

  private sub = new Subscription();
  readonly MAX_SEATS = 4;

  constructor(
    private searchService: SearchService,
    private seatLockService: SeatLockService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.countdown$ = this.seatLockService.countdown$;
    
    // Auto-release UI state when timer expires
    this.sub.add(
      this.countdown$.subscribe(seconds => {
        if (seconds === 0 && this.selectedSeatIds.size > 0) {
          this.selectedSeatIds.clear();
          this.errorMessage = 'Your seat reservation has expired.';
        }
      })
    );

    if (this.busId && this.date) {
      this.loadSeatMap();
    }
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  private loadSeatMap(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.searchService.getSeatMap(this.busId, this.date).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.buildLayout(response.seatStatuses, response.layoutConfig);
      },
      error: () => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load seat map. Please try again.';
      }
    });
  }

  /**
   * Build deck/row layout from seat statuses.
   * Uses layoutConfig JSON if available; otherwise groups by deck and seatNumber.
   */
  private buildLayout(seats: SeatAvailabilityDto[], layoutConfig?: string): void {
    // Group seats by deck
    const deckMap = new Map<string, SeatAvailabilityDto[]>();
    for (const seat of seats) {
      const deck = seat.deck ?? 'Lower';
      if (!deckMap.has(deck)) {
        deckMap.set(deck, []);
      }
      deckMap.get(deck)!.push(seat);
    }

    this.decks = [];
    for (const [deckName, deckSeats] of deckMap.entries()) {
      // Sort by seatNumber
      deckSeats.sort((a, b) => (a.seatNumber ?? 0) - (b.seatNumber ?? 0));

      // Group into rows of 4 (2 + aisle + 2)
      const rows: SeatRow[] = [];
      const seatsPerRow = 4;
      for (let i = 0; i < deckSeats.length; i += seatsPerRow) {
        const chunk = deckSeats.slice(i, i + seatsPerRow);
        // Insert aisle gap after index 1
        const rowSeats: (SeatAvailabilityDto | null)[] = [
          chunk[0] ?? null,
          chunk[1] ?? null,
          null,  // aisle
          chunk[2] ?? null,
          chunk[3] ?? null
        ];
        rows.push({ rowIndex: i / seatsPerRow, seats: rowSeats });
      }

      this.decks.push({ deckName, rows });
    }
  }

  selectSeat(seat: SeatAvailabilityDto | null): void {
    if (!seat) return;
    if (seat.status !== 'AVAILABLE') return;

    if (!this.authService.isLoggedIn) {
      this.showLoginModal = true;
      return;
    }

    // Toggle seat selection
    if (this.selectedSeatIds.has(seat.seatId)) {
      // Deselect seat and release lock
      this.seatLockService.releaseLockBySeatId(seat.seatId).subscribe({
        next: () => {
          this.selectedSeatIds.delete(seat.seatId);
          this.seatDeselected.emit(seat.seatId);
        },
        error: () => {
          // Clear locally even if API fails
          this.selectedSeatIds.delete(seat.seatId);
          this.seatDeselected.emit(seat.seatId);
        }
      });
    } else {
      // Check if maximum seats limit reached
      if (this.selectedSeatIds.size >= this.MAX_SEATS) {
        this.errorMessage = `Maximum ${this.MAX_SEATS} seats can be booked at once.`;
        return;
      }
      
      // Select new seat
      this.errorMessage = null;
      this.seatLockService.lockSeat({
        seatId: seat.seatId,
        busId: this.busId,
        journeyDate: this.date
      }).subscribe({
        next: () => {
          this.selectedSeatIds.add(seat.seatId);
          this.seatSelected.emit(seat);
        },
        error: (err: HttpErrorResponse) => {
          if (err.status === 409) {
            this.errorMessage = 'This seat was just taken. Please choose another.';
          } else {
            this.errorMessage = 'Failed to lock seat. Please try again.';
          }
        }
      });
    }
  }

  releaseSeat(): void {
    // Release all selected seats
    this.selectedSeatIds.forEach(seatId => {
      this.seatLockService.releaseLockBySeatId(seatId).subscribe({
        error: () => {
          // Clear locally even if API fails
          this.selectedSeatIds.delete(seatId);
        }
      });
    });
    this.selectedSeatIds.clear();
  }

  getSeatClass(seat: SeatAvailabilityDto | null): string {
    if (!seat) return '';

    if (this.selectedSeatIds.has(seat.seatId)) {
      return 'seat seat-selected';
    }

    switch (seat.status) {
      case 'BOOKED':
        return seat.passengerGender === 'Female'
          ? 'seat seat-female'
          : 'seat seat-male';
      case 'LOCKED':
        return 'seat seat-locked';
      case 'AVAILABLE':
      default:
        return 'seat seat-available';
    }
  }

  closeLoginModal(): void {
    this.showLoginModal = false;
  }

  formatTime(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  /**
   * Public method to programmatically deselect a seat
   * Used by parent component to ensure synchronization
   */
  public deselectSeat(seatId: string): void {
    if (this.selectedSeatIds.has(seatId)) {
      this.selectedSeatIds.delete(seatId);
      this.seatDeselected.emit(seatId);
    }
  }

  /**
   * Public method to check if a seat is selected
   */
  public isSeatSelected(seatId: string): boolean {
    return this.selectedSeatIds.has(seatId);
  }
}
