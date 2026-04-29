import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { OperatorService } from '../../../core/services/operator.service';
import { BusDetailDto, OperatorProfileDto } from '../../../shared/models/operator.models';

@Component({
  selector: 'app-operator-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  buses: BusDetailDto[] = [];
  allBookings: any[] = [];
  filteredBookings: any[] = [];
  selectedBusId: string | null = null;

  isLoading = true;
  errorMessage: string | null = null;
  operatorProfile: OperatorProfileDto | null = null;
  isProfileLoading = true;

  // Bus status control state
  isUpdatingBusStatus = false;
  busStatusMessage: string | null = null;

  constructor(private operatorService: OperatorService) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  private loadProfile(): void {
    this.operatorService.getProfile().subscribe({
      next: (profile) => {
        this.operatorProfile = profile;
        this.isProfileLoading = false;
        
        if (profile.isApproved) {
          this.loadData();
        } else {
          this.isLoading = false;
        }
      },
      error: (err) => {
        console.error('Failed to load operator profile', err);
        this.errorMessage = 'Failed to load operator profile.';
        this.isProfileLoading = false;
        this.isLoading = false;
      }
    });
  }

  private loadData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.operatorService.getBuses().subscribe({
      next: buses => {
        this.buses = buses;
      },
      error: () => {
        this.errorMessage = 'Failed to load buses.';
      }
    });

    this.operatorService.getBookings().subscribe({
      next: bookings => {
        this.allBookings = bookings;
        this.applyFilter();
        this.isLoading = false;
      },
      error: () => {
        this.allBookings = [];
        this.filteredBookings = [];
        this.isLoading = false;
      }
    });
  }

  onBusFilterChange(): void {
    this.applyFilter();
  }

  private applyFilter(): void {
    if (!this.selectedBusId) {
      this.filteredBookings = this.allBookings;
    } else {
      this.filteredBookings = this.allBookings.filter(
        b => b.busId === this.selectedBusId
      );
    }
  }

  // Bus status control methods
  async disableBus(busId: string): Promise<void> {
    if (!confirm('Are you sure you want to disable this bus? This will cancel all future bookings.')) {
      return;
    }

    this.isUpdatingBusStatus = true;
    this.busStatusMessage = null;

    try {
      await this.operatorService.disableBus(busId).toPromise();
      this.busStatusMessage = 'Bus disabled successfully';
      this.loadBuses(); // Refresh bus list
      setTimeout(() => this.busStatusMessage = null, 3000);
    } catch (error) {
      this.busStatusMessage = 'Failed to disable bus';
      setTimeout(() => this.busStatusMessage = null, 3000);
    } finally {
      this.isUpdatingBusStatus = false;
    }
  }

  async removeBus(busId: string): Promise<void> {
    if (!confirm('Are you sure you want to remove this bus? This will cancel all future bookings and cannot be undone.')) {
      return;
    }

    this.isUpdatingBusStatus = true;
    this.busStatusMessage = null;

    try {
      await this.operatorService.removeBus(busId).toPromise();
      this.busStatusMessage = 'Bus removed successfully';
      this.loadBuses(); // Refresh bus list
      setTimeout(() => this.busStatusMessage = null, 3000);
    } catch (error) {
      this.busStatusMessage = 'Failed to remove bus';
      setTimeout(() => this.busStatusMessage = null, 3000);
    } finally {
      this.isUpdatingBusStatus = false;
    }
  }

  getBusStatusBadgeClass(status?: string): string {
    switch (status) {
      case 'ACTIVE':   return 'bg-success';
      case 'PENDING':  return 'bg-warning';
      case 'DISABLED': return 'bg-secondary';
      case 'REMOVED':  return 'bg-danger';
      case 'REJECTED': return 'bg-danger';
      default:         return 'bg-secondary';
    }
  }

  getPassengerDetails(booking: any): string {
    if (!booking.seats || booking.seats.length === 0) {
      return 'No passenger details';
    }
    
    return booking.seats.map((seat: any) => 
      `${seat.passengerName} (${seat.passengerAge}, ${seat.passengerGender})`
    ).join(', ');
  }

  private loadBuses(): void {
    this.operatorService.getBuses().subscribe({
      next: buses => {
        this.buses = buses;
      },
      error: () => {
        // Error handled in main loadData method
      }
    });
  }
}
