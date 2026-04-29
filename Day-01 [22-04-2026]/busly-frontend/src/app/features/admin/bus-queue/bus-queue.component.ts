import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../../core/services/admin.service';
import { BusDto, OperatorDto } from '../../../shared/models/admin.models';

@Component({
  selector: 'app-bus-queue',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bus-queue.component.html'
})
export class BusQueueComponent implements OnInit {
  buses: BusDto[] = [];
  errorMessage: string | null = null;
  selectedBus: BusDto | null = null;
  selectedOperator: OperatorDto | null = null;
  operatorCache: { [key: string]: OperatorDto } = {};
  viewMode: 'pending' | 'all' = 'pending';

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadBuses();
  }

  loadBuses(): void {
    const obs = this.viewMode === 'pending' 
      ? this.adminService.getPendingBuses() 
      : this.adminService.getAllBuses();

    obs.subscribe({
      next: (data) => {
        this.buses = data;
        this.loadOperatorData();
      },
      error: () => (this.errorMessage = `Failed to load ${this.viewMode} buses.`)
    });
  }

  loadOperatorData(): void {
    const uniqueOperatorIds = [...new Set(this.buses.map(bus => bus.operatorId).filter((id): id is string => id !== undefined))];
    
    if (uniqueOperatorIds.length === 0) return;
    
    // Try to load all operators and cache them for lookup
    this.adminService.getAllOperators().subscribe({
      next: (operators) => {
        operators.forEach(operator => {
          this.operatorCache[operator.id] = operator;
        });
      },
      error: () => {
        console.warn('Failed to load operators list, falling back to individual requests');
        // Fallback: try individual requests
        this.loadOperatorsIndividually(uniqueOperatorIds);
      }
    });
  }

  loadOperatorsIndividually(operatorIds: string[]): void {
    // No individual operator endpoint exists — use getAllOperators instead
    this.adminService.getAllOperators().subscribe({
      next: (operators) => {
        operators.forEach(operator => {
          this.operatorCache[operator.id] = operator;
        });
      },
      error: () => console.warn('Failed to load operators for cache')
    });
  }

  setViewMode(mode: 'pending' | 'all'): void {
    this.viewMode = mode;
    this.selectedBus = null;
    this.loadBuses();
  }

  approve(id: string): void {
    this.adminService.approveBus(id).subscribe({
      next: () => {
        if (this.viewMode === 'pending') {
          this.buses = this.buses.filter(bus => bus.id !== id);
        } else {
          const bus = this.buses.find(b => b.id === id);
          if (bus) bus.status = 'ACTIVE';
        }
      },
      error: () => (this.errorMessage = 'Failed to approve bus.')
    });
  }

  reject(id: string): void {
    this.adminService.rejectBus(id).subscribe({
      next: () => {
        if (this.viewMode === 'pending') {
          this.buses = this.buses.filter(bus => bus.id !== id);
        } else {
          const bus = this.buses.find(b => b.id === id);
          if (bus) bus.status = 'REJECTED';
        }
      },
      error: () => (this.errorMessage = 'Failed to reject bus.')
    });
  }

  toggleStatus(id: string): void {
    this.adminService.toggleBusStatus(id).subscribe({
      next: () => {
        const bus = this.buses.find(b => b.id === id);
        if (bus) {
          bus.status = bus.status === 'ACTIVE' ? 'DISABLED' : 'ACTIVE';
        }
      },
      error: (err) => (this.errorMessage = err.error?.message || 'Failed to toggle bus status.')
    });
  }

  viewDetails(bus: BusDto): void {
    this.selectedBus = bus;
    this.selectedOperator = null;
    
    // Load operator details if operatorId is available
    if (bus.operatorId) {
      const operatorId = bus.operatorId;
      // Check if operator data is already in cache
      if (this.operatorCache[operatorId]) {
        this.selectedOperator = this.operatorCache[operatorId];
      } else {
        // Try to load from getAllOperators first
        this.adminService.getAllOperators().subscribe({
          next: (operators) => {
            const operator = operators.find(op => op.id === operatorId);
            if (operator) {
              this.selectedOperator = operator;
              this.operatorCache[operatorId] = operator;
            } else {
              // Fallback to individual request
              this.loadOperatorForBus(operatorId);
            }
          },
          error: () => {
            // Fallback to individual request
            this.loadOperatorForBus(operatorId);
          }
        });
      }
    }
  }

  loadOperatorForBus(operatorId: string): void {
    // No individual operator endpoint — look up from getAllOperators
    this.adminService.getAllOperators().subscribe({
      next: (operators) => {
        const operator = operators.find(op => op.id === operatorId);
        if (operator) {
          this.selectedOperator = operator;
          this.operatorCache[operatorId] = operator;
        }
      },
      error: () => {
        console.warn('Failed to load operator details for bus:', this.selectedBus?.id);
      }
    });
  }

  closeModal(): void {
    this.selectedBus = null;
    this.selectedOperator = null;
  }

  getOperatorCompanyName(busId: string): string {
    const bus = this.buses.find(b => b.id === busId);
    if (bus?.operatorId && this.operatorCache[bus.operatorId]) {
      return this.operatorCache[bus.operatorId].companyName;
    }
    return '';
  }

  // Helper methods for operating days display
  isBusOperatingOnDay(bus: BusDto, day: number): boolean {
    if (!bus.operatingDays) return false;
    const operatingDay = bus.operatingDays.find(d => d.dayOfWeek === day);
    return operatingDay?.isActive || false;
  }

  getDayBadgeClass(bus: BusDto, day: number): string {
    return this.isBusOperatingOnDay(bus, day) ? 'bg-success' : 'bg-secondary';
  }

  // View all buses by operator
  viewOperatorBuses(operator: OperatorDto): void {
    this.adminService.getBusesByOperator(operator.id).subscribe({
      next: (buses) => {
        this.buses = buses;
        this.selectedOperator = operator;
        this.selectedBus = null;
      },
      error: () => (this.errorMessage = 'Failed to load buses for operator.')
    });
  }

  // Back to all buses view
  showAllBuses(): void {
    this.loadBuses();
    this.selectedOperator = null;
  }
}
