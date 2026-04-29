import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OperatorService } from '../../../core/services/operator.service';
import { SeatConfigDto, SeatItemDto } from '../../../shared/models/operator.models';
import { Router } from '@angular/router';

@Component({
  selector: 'app-layout-builder',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './layout-builder.component.html'
})
export class LayoutBuilderComponent implements OnInit {
  form: FormGroup;
  previewSeats: SeatItemDto[] = [];
  previewConfig: SeatConfigDto | null = null;
  successMessage = '';
  errorMessage = '';
  isSubmitting = false;
  returnToBusRegistration = false;

  constructor(
    private fb: FormBuilder,
    private operatorService: OperatorService,
    private router: Router
  ) {
    this.form = this.fb.group({
      layoutName: ['', Validators.required],
      rows:       [null, [Validators.required, Validators.min(1), Validators.max(20)]],
      cols:       [null, [Validators.required, Validators.min(1), Validators.max(10)]],
      lowerDeck:  [true],
      upperDeck:  [false]
    });
  }

  ngOnInit(): void {
    // Check if we should return to bus registration after creating layout
    this.returnToBusRegistration = sessionStorage.getItem('returnToBusRegistration') === 'true';
  }

  generateGrid(): void {
    if (this.form.get('rows')?.invalid || this.form.get('cols')?.invalid) {
      this.form.get('rows')?.markAsTouched();
      this.form.get('cols')?.markAsTouched();
      return;
    }

    const rows: number = Number(this.form.value.rows);
    const cols: number = Number(this.form.value.cols);
    const decks: string[] = [];

    if (this.form.value.lowerDeck) decks.push('lower');
    if (this.form.value.upperDeck) decks.push('upper');

    if (decks.length === 0) {
      this.errorMessage = 'Please select at least one deck.';
      return;
    }

    this.errorMessage = '';
    const seats: SeatItemDto[] = [];
    let seatNumber = 1;

    for (const deck of decks) {
      for (let r = 1; r <= rows; r++) {
        for (let c = 1; c <= cols; c++) {
          const type = (c === 1 || c === cols) ? 'window' : 'aisle';
          seats.push({ seatNumber: seatNumber++, row: r, col: c, type, deck });
        }
      }
    }

    this.previewConfig = { rows, cols, decks, seats };
    this.previewSeats = seats;
  }

  /** Returns seats for a given deck and row for the visual grid. */
  seatsForRow(deck: string, row: number): SeatItemDto[] {
    return this.previewSeats.filter(s => s.deck === deck && s.row === row);
  }

  get deckList(): string[] {
    return this.previewConfig?.decks ?? [];
  }

  get rowNumbers(): number[] {
    if (!this.previewConfig) return [];
    return Array.from({ length: this.previewConfig.rows }, (_, i) => i + 1);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (!this.previewConfig) {
      this.errorMessage = 'Please generate a preview before saving.';
      return;
    }

    this.isSubmitting = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.operatorService.createLayout({
      layoutName: this.form.value.layoutName,
      seatConfig: this.previewConfig
    }).subscribe({
      next: () => {
        this.successMessage = 'Layout saved successfully!';
        
        // Clear the return flag and form data
        sessionStorage.removeItem('returnToBusRegistration');
        
        if (this.returnToBusRegistration) {
          // Return to bus registration page
          setTimeout(() => this.router.navigate(['/operator/buses/register']), 1500);
        } else {
          // Go to layouts list (default behavior)
          setTimeout(() => this.router.navigate(['/operator/layouts']), 1500);
        }
      },
      error: () => {
        this.errorMessage = 'Failed to save layout. Please try again.';
        this.isSubmitting = false;
      }
    });
  }

  isInvalid(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }
}
