import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../../core/services/admin.service';
import { OperatorDto } from '../../../shared/models/admin.models';

@Component({
  selector: 'app-operator-queue',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './operator-queue.component.html'
})
export class OperatorQueueComponent implements OnInit {
  operators: OperatorDto[] = [];
  errorMessage: string | null = null;
  selectedOperator: OperatorDto | null = null;
  viewMode: 'pending' | 'all' = 'pending';

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadOperators();
  }

  loadOperators(): void {
    const obs = this.viewMode === 'pending'
      ? this.adminService.getPendingOperators()
      : this.adminService.getAllOperators();

    obs.subscribe({
      next: (data) => (this.operators = data),
      error: () => (this.errorMessage = `Failed to load ${this.viewMode} operators.`)
    });
  }

  setViewMode(mode: 'pending' | 'all'): void {
    this.viewMode = mode;
    this.selectedOperator = null;
    this.loadOperators();
  }

  approve(id: string): void {
    this.adminService.approveOperator(id).subscribe({
      next: () => {
        if (this.viewMode === 'pending') {
          this.operators = this.operators.filter(op => op.id !== id);
        } else {
          const op = this.operators.find(o => o.id === id);
          if (op) op.status = 'APPROVED';
        }
      },
      error: () => (this.errorMessage = 'Failed to approve operator.')
    });
  }

  reject(id: string): void {
    this.adminService.rejectOperator(id).subscribe({
      next: () => {
        if (this.viewMode === 'pending') {
          this.operators = this.operators.filter(op => op.id !== id);
        } else {
          const op = this.operators.find(o => o.id === id);
          if (op) op.status = 'REJECTED';
        }
      },
      error: () => (this.errorMessage = 'Failed to reject operator.')
    });
  }

  toggleStatus(id: string): void {
    this.adminService.toggleOperator(id).subscribe({
      next: () => {
        const op = this.operators.find(o => o.id === id);
        if (op) {
          op.status = op.status === 'APPROVED' ? 'DISABLED' : 'APPROVED';
        }
      },
      error: (err) => (this.errorMessage = err.error?.message || 'Failed to toggle operator status.')
    });
  }

  viewDetails(op: OperatorDto): void {
    this.selectedOperator = op;
  }

  closeModal(): void {
    this.selectedOperator = null;
  }
}
