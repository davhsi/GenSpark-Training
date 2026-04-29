import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface CancelBookingDialogData {
  pnr: string;
  refundAmount: number;
  refundPercentage: number;
}

@Component({
  selector: 'app-cancel-booking-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cancel-booking-dialog.component.html',
  styleUrls: ['./cancel-booking-dialog.component.css']
})
export class CancelBookingDialogComponent {
  @Input() data: CancelBookingDialogData = { pnr: '', refundAmount: 0, refundPercentage: 0 };
  @Output() cancel = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<void>();

  onCancel(): void {
    this.cancel.emit();
  }

  onConfirm(): void {
    this.confirm.emit();
  }
}
