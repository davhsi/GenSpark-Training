import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { SeatMapComponent } from '../../../shared/components/seat-map/seat-map.component';
import { SeatLockService } from '../../../core/services/seat-lock.service';
import { SearchService } from '../../../core/services/search.service';
import { Subscription, Observable } from 'rxjs';
import { BusSearchResultDto, SeatAvailabilityDto } from '../../../shared/models/search.models';

interface SelectedSeat {
  seatId: string;
  seatNumber: string;
  price: number;
}

@Component({
  selector: 'app-seat-selection',
  standalone: true,
  imports: [CommonModule, RouterModule, SeatMapComponent],
  templateUrl: './seat-selection.component.html'
})
export class SeatSelectionComponent implements OnInit, OnDestroy {
  @ViewChild(SeatMapComponent) seatMapComponent!: SeatMapComponent;
  
  busId: string = '';
  date: string = '';
  selectedSeats: SelectedSeat[] = [];
  busDetails: BusSearchResultDto | null = null;
  countdown$!: Observable<number>;
  discount = 0;
  private subscription = new Subscription();
  
  readonly MAX_SEATS = 4;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private seatLockService: SeatLockService,
    private searchService: SearchService
  ) {}

  ngOnInit(): void {
    this.busId = this.route.snapshot.paramMap.get('busId') ?? '';
    this.date = this.route.snapshot.queryParamMap.get('date') ?? '';
    
    this.countdown$ = this.seatLockService.countdown$;
    
    // Load bus details
    this.loadBusDetails();
    
    // Add navigation warning when seat is locked
    this.subscription.add(
      this.countdown$.subscribe(seconds => {
        if (seconds > 0) {
          this.setupNavigationWarning();
        } else {
          this.removeNavigationWarning();
        }
      })
    );
  }

  private loadBusDetails(): void {
    this.searchService.getBusDetails(this.busId).subscribe({
      next: (bus: BusSearchResultDto) => {
        this.busDetails = bus;
      },
      error: () => {
        // Fallback to mock data
        this.busDetails = {
          busId: this.busId,
          busName: 'Express Bus',
          busNumber: 'EXP-001',
          operatorName: 'Busly Tours',
          sourceCity: 'New York',
          destinationCity: 'Boston',
          basePrice: 35.50,
          availableSeats: 40
        };
      }
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.removeNavigationWarning();
    // Release all seat locks when component is destroyed
    this.releaseAllSeatLocks();
  }

  private setupNavigationWarning(): void {
    window.addEventListener('beforeunload', this.handleBeforeUnload);
  }

  private removeNavigationWarning(): void {
    window.removeEventListener('beforeunload', this.handleBeforeUnload);
  }

  private handleBeforeUnload = (event: BeforeUnloadEvent): void => {
    if (this.seatLockService.activeLock) {
      event.preventDefault();
      event.returnValue = 'You have a seat reserved. Are you sure you want to leave? Your reservation will be lost.';
      return event.returnValue;
    }
  };

  onSeatSelected(seat: any): void {
    // Check if seat is already selected
    const existingSeat = this.selectedSeats.find(s => s.seatId === seat.seatId);
    if (existingSeat) {
      return; // Seat already selected
    }
    
    // Check if maximum seats limit reached
    if (this.selectedSeats.length >= this.MAX_SEATS) {
      return; // Maximum seats reached
    }
    
    // Add seat to selected seats
    this.selectedSeats.push({
      seatId: seat.seatId,
      seatNumber: seat.seatNumber || seat.seatId,
      price: this.busDetails?.basePrice || 35.50
    });
  }

  onSeatDeselected(seatId: string): void {
    // Remove seat from selected seats array
    this.selectedSeats = this.selectedSeats.filter(s => s.seatId !== seatId);
    
    // Release the seat lock to ensure server-side synchronization
    this.seatLockService.releaseLockBySeatId(seatId).subscribe({
      error: () => {
        // Even if API fails, keep the UI updated
        console.warn('Failed to release seat lock, but UI updated');
      }
    });
  }

  removeSeat(seatId: string): void {
    // Remove seat from selected seats array
    this.selectedSeats = this.selectedSeats.filter(s => s.seatId !== seatId);
    
    // Update seat map component to ensure UI synchronization
    if (this.seatMapComponent) {
      this.seatMapComponent.deselectSeat(seatId);
    }
    
    // Release the seat lock
    this.seatLockService.releaseLockBySeatId(seatId).subscribe({
      error: () => {
        // Even if API fails, keep the UI updated
        console.warn('Failed to release seat lock, but UI updated');
      }
    });
  }

  clearAllSeats(): void {
    // Get all seat IDs before clearing
    const seatIds = this.selectedSeats.map(s => s.seatId);
    
    // Clear local array immediately
    this.selectedSeats = [];
    
    // Update seat map component to clear all selections
    if (this.seatMapComponent) {
      this.seatMapComponent.releaseSeat();
    }
    
    // Release all seat locks
    seatIds.forEach(seatId => {
      this.seatLockService.releaseLockBySeatId(seatId).subscribe({
        error: () => {
          console.warn(`Failed to release lock for seat ${seatId}`);
        }
      });
    });
  }

  private releaseAllSeatLocks(): void {
    // Release all active seat locks from the service
    this.seatLockService.activeLocks.forEach((lock, seatId) => {
      this.seatLockService.releaseLockBySeatId(seatId).subscribe();
    });
    // Also clear any locally selected seats
    this.selectedSeats = [];
  }

  proceedToBooking(): void {
    if (this.selectedSeats.length === 0) return;

    // Navigate to booking summary with all selected seats
    const seatIds = this.selectedSeats.map(s => s.seatId);
    this.router.navigate(['/booking/summary'], {
      queryParams: {
        busId: this.busId,
        seatId: seatIds, // Pass as array to match booking summary expectation
        date: this.date,
        totalAmount: this.calculateTotal()
      }
    });
  }

  calculateBasePrice(): number {
    return this.selectedSeats.reduce((total, seat) => total + seat.price, 0);
  }

  calculateTotal(): number {
    const basePrice = this.calculateBasePrice();
    return Math.max(0, basePrice - this.discount);
  }

  formatTime(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${remainingSeconds.toString().padStart(2, '0')}`;
  }

  formatDate(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
      weekday: 'short', 
      month: 'short', 
      day: 'numeric' 
    });
  }
}
