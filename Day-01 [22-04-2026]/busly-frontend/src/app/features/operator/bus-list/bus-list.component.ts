import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { OperatorService } from '../../../core/services/operator.service';
import { BusDetailDto, OperatorProfileDto, UpdateOperatingDaysRequest } from '../../../shared/models/operator.models';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OperatingDaysComponent } from './operating-days.component';

@Component({
  selector: 'app-bus-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, OperatingDaysComponent],
  templateUrl: './bus-list.component.html'
})
export class BusListComponent implements OnInit {
  buses: BusDetailDto[] = [];
  errorMessage = '';
  isLoading = true;
  operatorProfile: OperatorProfileDto | null = null;

  @ViewChild(OperatingDaysComponent) operatingDaysComponent?: OperatingDaysComponent;

  // Staff Modal State
  isStaffModalOpen = false;
  isSavingStaff = false;
  staffForm: FormGroup;
  selectedBusId: string | null = null;

  // Price Modal State
  isPriceModalOpen = false;
  isSavingPrice = false;
  priceForm: FormGroup;
  selectedBusForPrice: BusDetailDto | null = null;

  // Operating Days Modal State
  isOperatingDaysModalOpen = false;
  selectedBusForDays: BusDetailDto | null = null;

  constructor(
    private operatorService: OperatorService,
    private fb: FormBuilder
  ) {
    this.staffForm = this.fb.group({
      driverName: [''],
      driverPhone: [''],
      conductorName: [''],
      conductorPhone: ['']
    });
    this.priceForm = this.fb.group({
      basePrice: [null, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  private loadProfile(): void {
    this.operatorService.getProfile().subscribe({
      next: (profile) => {
        this.operatorProfile = profile;
        if (profile.isApproved) {
          this.loadBuses();
        } else {
          this.isLoading = false;
          this.errorMessage = this.getApprovalMessage(profile);
        }
      },
      error: () => {
        this.errorMessage = 'Failed to load operator profile.';
        this.isLoading = false;
      }
    });
  }

  private getApprovalMessage(profile: OperatorProfileDto): string {
    if (profile.isPending) {
      return 'Your operator account is pending approval. You cannot manage buses until approved by an administrator.';
    } else if (profile.isRejected) {
      return 'Your operator account has been rejected. Please contact support for more information.';
    } else if (profile.isDisabled) {
      return 'Your operator account has been disabled. Please contact support for more information.';
    }
    return 'Your account status does not allow bus management.';
  }

  loadBuses(): void {
    this.errorMessage = '';
    this.isLoading = true;
    this.operatorService.getBuses().subscribe({
      next: (data) => {
        this.buses = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        if (err.status === 401) {
          this.errorMessage = 'Please log in again to view your buses.';
        } else {
          this.errorMessage = 'Could not load buses. Please try again later.';
        }
      }
    });
  }

  statusBadgeClass(status: string | undefined): string {
    switch (status?.toUpperCase()) {
      case 'PENDING':  return 'bg-warning text-dark';
      case 'ACTIVE':   return 'bg-success';
      case 'DISABLED': return 'bg-secondary';
      case 'REMOVED':  return 'bg-danger';
      case 'REJECTED': return 'bg-danger';
      default:         return 'bg-light text-dark';
    }
  }

  disable(id: string): void {
    this.operatorService.disableBus(id).subscribe({
      next: () => this.loadBuses(),
      error: () => (this.errorMessage = 'Failed to disable bus.')
    });
  }

  remove(id: string): void {
    if (!confirm('Are you sure you want to remove this bus? This action cannot be undone.')) return;
    this.operatorService.removeBus(id).subscribe({
      next: () => this.loadBuses(),
      error: () => (this.errorMessage = 'Failed to remove bus.')
    });
  }

  canActOnBus(status: string | undefined): boolean {
    const s = status?.toUpperCase();
    return s === 'ACTIVE' || s === 'PENDING';
  }

  // --- Staff Modal ---

  editStaff(bus: BusDetailDto): void {
    this.selectedBusId = bus.id;
    this.staffForm.patchValue({
      driverName: bus.driverName || '',
      driverPhone: bus.driverPhone || '',
      conductorName: bus.conductorName || '',
      conductorPhone: bus.conductorPhone || ''
    });
    this.isStaffModalOpen = true;
  }

  closeStaffModal(): void {
    this.isStaffModalOpen = false;
    this.selectedBusId = null;
    this.staffForm.reset();
  }

  saveStaff(): void {
    if (!this.selectedBusId) return;
    this.isSavingStaff = true;
    this.operatorService.updateBusStaff(this.selectedBusId, this.staffForm.value).subscribe({
      next: () => {
        this.isSavingStaff = false;
        this.closeStaffModal();
        this.loadBuses();
      },
      error: () => {
        this.isSavingStaff = false;
        alert('Failed to update staff details.');
      }
    });
  }

  // --- Price Modal ---

  editPrice(bus: BusDetailDto): void {
    this.selectedBusForPrice = bus;
    this.priceForm.patchValue({ basePrice: bus.basePrice });
    this.isPriceModalOpen = true;
  }

  closePriceModal(): void {
    this.isPriceModalOpen = false;
    this.selectedBusForPrice = null;
    this.priceForm.reset();
  }

  savePrice(): void {
    if (!this.selectedBusForPrice || this.priceForm.invalid) return;
    this.isSavingPrice = true;
    const newPrice = Math.round(Number(this.priceForm.value.basePrice) * 100) / 100;
    this.operatorService.updateBusPrice(this.selectedBusForPrice.id, { basePrice: newPrice }).subscribe({
      next: () => {
        this.isSavingPrice = false;
        this.closePriceModal();
        this.loadBuses();
      },
      error: () => {
        this.isSavingPrice = false;
        alert('Failed to update price. Please try again.');
      }
    });
  }

  // --- Operating Days ---

  showOperatingDaysModal(bus: BusDetailDto): void {
    this.selectedBusForDays = bus;
    this.isOperatingDaysModalOpen = true;
  }

  closeOperatingDaysModal(): void {
    this.isOperatingDaysModalOpen = false;
    this.selectedBusForDays = null;
  }

  onOperatingDaysUpdated(request: UpdateOperatingDaysRequest): void {
    this.operatorService.updateOperatingDays(request.busId, request).subscribe({
      next: () => {
        this.operatingDaysComponent?.onSaveSuccess();
        this.closeOperatingDaysModal();
        this.loadBuses();
      },
      error: (err) => {
        console.error('Failed to update operating days:', err);
        this.operatingDaysComponent?.onSaveError('Failed to update operating days. Please try again.');
      }
    });
  }

  isBusOperatingOnDay(bus: BusDetailDto, day: number): boolean {
    if (!bus.operatingDays) return false;
    const operatingDay = bus.operatingDays.find(d => d.dayOfWeek === day);
    return operatingDay?.isActive || false;
  }

  getDayBadgeClass(bus: BusDetailDto, day: number): string {
    return this.isBusOperatingOnDay(bus, day) ? 'bg-success' : 'bg-secondary';
  }
}
