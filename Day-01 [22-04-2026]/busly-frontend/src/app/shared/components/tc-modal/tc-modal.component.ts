import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { TcStatusDto } from '../../../shared/models/auth.models';

@Component({
  selector: 'app-tc-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tc-modal.component.html'
})
export class TcModalComponent implements OnInit {
  /** The full Terms & Conditions text to display. */
  @Input() content: string = '';

  /** Emitted after the user accepts and the API call succeeds. */
  @Output() accepted = new EventEmitter<void>();

  /** Emitted when the user clicks Decline. */
  @Output() declined = new EventEmitter<void>();

  isAccepting = false;
  errorMessage: string | null = null;
  tcStatus: TcStatusDto | null = null;
  loading = true;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.loadTcStatus();
  }

  loadTcStatus(): void {
    this.authService.getTcStatus().subscribe({
      next: (status) => {
        this.tcStatus = status;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        // Continue without T&C status - this shouldn't break the modal
      }
    });
  }

  onAccept(): void {
    this.isAccepting = true;
    this.errorMessage = null;

    this.authService.acceptTc().subscribe({
      next: () => {
        this.isAccepting = false;
        this.accepted.emit();
      },
      error: () => {
        this.isAccepting = false;
        this.errorMessage = 'Failed to accept Terms & Conditions. Please try again.';
      }
    });
  }

  onDecline(): void {
    this.declined.emit();
  }
}
