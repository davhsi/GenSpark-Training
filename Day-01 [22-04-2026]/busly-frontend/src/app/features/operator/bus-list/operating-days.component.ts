import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OperatingDayDto, UpdateOperatingDaysRequest } from '../../../shared/models/operator.models';

@Component({
  selector: 'app-operating-days',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './operating-days.component.html'
})
export class OperatingDaysComponent {
  @Input() busId: string = '';
  @Input() operatingDays: OperatingDayDto[] = [];
  @Output() operatingDaysUpdated = new EventEmitter<UpdateOperatingDaysRequest>();

  isEditing = false;
  isSaving = false;
  errorMessage = '';

  // Local copy for editing
  private originalOperatingDays: OperatingDayDto[] = [];
  editingOperatingDays: OperatingDayDto[] = [];

  // Day names for display
  dayNames = [
    { value: 1, name: 'Monday' },
    { value: 2, name: 'Tuesday' },
    { value: 3, name: 'Wednesday' },
    { value: 4, name: 'Thursday' },
    { value: 5, name: 'Friday' },
    { value: 6, name: 'Saturday' },
    { value: 7, name: 'Sunday' }
  ];

  ngOnChanges(): void {
    // Initialize operating days if not provided
    if (!this.operatingDays || this.operatingDays.length === 0) {
      this.operatingDays = this.dayNames.map(day => ({
        dayOfWeek: day.value,
        isActive: true
      }));
    }
    
    this.originalOperatingDays = [...this.operatingDays];
    this.editingOperatingDays = [...this.operatingDays];
  }

  getDayName(dayOfWeek: number): string {
    const day = this.dayNames.find(d => d.value === dayOfWeek);
    return day ? day.name : `Day ${dayOfWeek}`;
  }

  startEditing(): void {
    this.isEditing = true;
    this.errorMessage = '';
    this.editingOperatingDays = [...this.operatingDays];
  }

  cancelEditing(): void {
    this.isEditing = false;
    this.editingOperatingDays = [...this.originalOperatingDays];
    this.errorMessage = '';
  }

  saveOperatingDays(): void {
    this.isSaving = true;
    this.errorMessage = '';

    const updateRequest: UpdateOperatingDaysRequest = {
      busId: this.busId,
      operatingDays: this.editingOperatingDays
    };

    this.operatingDaysUpdated.emit(updateRequest);
  }

  // Called by parent component after successful save
  onSaveSuccess(): void {
    this.isSaving = false;
    this.isEditing = false;
    this.originalOperatingDays = [...this.editingOperatingDays];
    this.operatingDays = [...this.editingOperatingDays];
  }

  // Called by parent component if save fails
  onSaveError(error: string): void {
    this.isSaving = false;
    this.errorMessage = error;
  }

  toggleDay(dayOfWeek: number): void {
    const day = this.editingOperatingDays.find(d => d.dayOfWeek === dayOfWeek);
    if (day) {
      day.isActive = !day.isActive;
    }
  }

  selectAllDays(): void {
    this.editingOperatingDays.forEach(day => {
      day.isActive = true;
    });
  }

  deselectAllDays(): void {
    this.editingOperatingDays.forEach(day => {
      day.isActive = false;
    });
  }

  getActiveDaysCount(): number {
    return this.operatingDays.filter(day => day.isActive).length;
  }

  getEditingActiveDaysCount(): number {
    return this.editingOperatingDays.filter(day => day.isActive).length;
  }

  // Helper method for template - check if day is active in display mode
  isDayActive(dayOfWeek: number): boolean {
    if (!this.operatingDays) return false;
    const operatingDay = this.operatingDays.find(d => d.dayOfWeek === dayOfWeek);
    return operatingDay?.isActive || false;
  }

  // Helper method for template - get badge class for display mode
  getDayBadgeClass(dayOfWeek: number): string {
    return this.isDayActive(dayOfWeek) ? 'bg-success' : 'bg-secondary';
  }

  // Helper method for template - check if day is active in editing mode
  isEditingDayActive(dayOfWeek: number): boolean {
    if (!this.editingOperatingDays) return false;
    const operatingDay = this.editingOperatingDays.find(d => d.dayOfWeek === dayOfWeek);
    return operatingDay?.isActive || false;
  }
}
