import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { OperatorService } from '../../../core/services/operator.service';
import { LayoutDto, OperatorProfileDto } from '../../../shared/models/operator.models';

@Component({
  selector: 'app-layout-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container-fluid">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h3 class="fw-bold mb-0">Seat Layouts</h3>
        <a routerLink="/operator/layouts/new" class="btn btn-primary" *ngIf="operatorProfile?.isApproved">
          + Create New Layout
        </a>
      </div>

      <!-- Loading State -->
      <div *ngIf="isLoading" class="text-center py-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading layouts…</span>
        </div>
      </div>

      <!-- Error/Approval Status Messages -->
      <div *ngIf="!isLoading && errorMessage" class="alert" [class.alert-danger]="!operatorProfile || operatorProfile.isApproved" [class.alert-warning]="operatorProfile && !operatorProfile.isApproved">
        <div *ngIf="operatorProfile && !operatorProfile.isApproved" class="d-flex align-items-center">
          <div class="me-3">
            <div class="spinner-border text-warning me-2" role="status" *ngIf="operatorProfile.isPending">
              <span class="visually-hidden">Loading…</span>
            </div>
            <i class="bi bi-exclamation-triangle-fill me-2" *ngIf="operatorProfile.isRejected || operatorProfile.isDisabled"></i>
          </div>
          <div>
            <h6 class="alert-heading mb-1" *ngIf="operatorProfile.isPending">Account Pending Approval</h6>
            <h6 class="alert-heading mb-1" *ngIf="operatorProfile.isRejected">Account Rejected</h6>
            <h6 class="alert-heading mb-1" *ngIf="operatorProfile.isDisabled">Account Disabled</h6>
            <p class="mb-0">{{ errorMessage }}</p>
          </div>
        </div>
        <div *ngIf="!operatorProfile || operatorProfile.isApproved">
          {{ errorMessage }}
        </div>
      </div>

      <div class="row g-4" *ngIf="!isLoading && !errorMessage && operatorProfile?.isApproved && layouts.length > 0">
        <div class="col-md-4 col-xl-3" *ngFor="let layout of layouts">
          <div class="card h-100 shadow-sm border-0 hover-lift">
            <div class="card-body">
              <div class="d-flex align-items-center mb-3">
                <div class="layout-icon me-3">
                  <span style="font-size: 1.5rem;">🪑</span>
                </div>
                <h5 class="card-title mb-0 fw-bold text-truncate">{{ layout.layoutName }}</h5>
              </div>
              <p class="card-text text-muted mb-4">
                Total Capacity: <span class="fw-bold text-dark">{{ layout.totalSeats }} Seats</span>
              </p>
              <div class="d-flex gap-2 mt-4">
                <button class="btn btn-outline-primary btn-sm fw-bold flex-grow-1" (click)="previewLayout(layout)">
                  View Map
                </button>
                <button class="btn btn-outline-danger btn-sm" (click)="deleteLayout(layout.id)">
                  🗑️
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Preview Modal -->
      <div class="modal fade show d-block" *ngIf="selectedLayout" style="background: rgba(0,0,0,0.5);">
        <div class="modal-dialog modal-lg modal-dialog-centered">
          <div class="modal-content border-0 shadow-lg">
            <div class="modal-header bg-dark text-white border-0">
              <h5 class="modal-title fw-bold">{{ selectedLayout.layoutName }} Preview</h5>
              <button type="button" class="btn-close btn-close-white" (click)="selectedLayout = null"></button>
            </div>
            <div class="modal-body p-4 bg-light">
              
              <div class="deck-container mb-4" *ngFor="let deck of deckList">
                <h6 class="text-uppercase fw-bold text-muted small mb-3">{{ deck }} Deck</h6>
                <div class="bus-grid-scroll">
                  <div class="bus-grid p-3 bg-white rounded shadow-sm">
                    <div class="bus-row d-flex justify-content-center mb-2" *ngFor="let row of rowNumbers">
                      <div 
                        *ngFor="let seat of seatsForRow(deck, row)"
                        class="seat shadow-sm"
                        [class.window]="seat.type === 'window'"
                        [class.aisle]="seat.type === 'aisle'"
                        [title]="'Seat ' + seat.seatNumber"
                      >
                        {{ seat.seatNumber }}
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div class="d-flex justify-content-center gap-4 mt-3 small text-muted">
                <div class="d-flex align-items-center"><span class="legend-box window me-1"></span> Window</div>
                <div class="d-flex align-items-center"><span class="legend-box aisle me-1"></span> Aisle</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div *ngIf="!isLoading && !errorMessage && operatorProfile?.isApproved && layouts.length === 0" class="text-center py-5">
        <div class="mb-3" style="font-size: 3rem;">🪑</div>
        <h4 class="fw-bold">No Layouts Found</h4>
        <p class="text-muted">You haven't created any seat configurations yet.</p>
        <a routerLink="/operator/layouts/new" class="btn btn-primary mt-2">
          Design Your First Layout
        </a>
      </div>
    </div>
  `,
  styles: [`
    .hover-lift {
      transition: transform 0.2s ease, box-shadow 0.2s ease;
    }
    .hover-lift:hover {
      transform: translateY(-5px);
      box-shadow: 0 10px 20px rgba(0,0,0,0.1) !important;
    }
    .layout-icon {
      width: 48px;
      height: 48px;
      background: rgba(99, 102, 241, 0.1);
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .bus-grid {
      display: inline-block;
      border: 2px solid #dee2e6;
      border-radius: 8px;
    }
    .seat {
      width: 40px;
      height: 40px;
      margin: 4px;
      background: #f8f9fa;
      border: 1px solid #ced4da;
      border-radius: 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      font-weight: bold;
      color: #6c757d;
    }
    .seat.window { border-left: 4px solid #0d6efd; }
    .seat.aisle { border-left: 4px solid #ffc107; }
    .legend-box {
      width: 16px;
      height: 16px;
      border-radius: 3px;
      display: inline-block;
    }
    .legend-box.window { background: #0d6efd; }
    .legend-box.aisle { background: #ffc107; }
    .bus-grid-scroll {
      overflow-x: auto;
      text-align: center;
    }
  `]
})
export class LayoutListComponent implements OnInit {
  layouts: LayoutDto[] = [];
  errorMessage = '';
  isLoading = true;
  operatorProfile: OperatorProfileDto | null = null;
  
  selectedLayout: any = null;
  deckList: string[] = [];
  rowNumbers: number[] = [];

  constructor(private operatorService: OperatorService) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  private loadProfile(): void {
    this.operatorService.getProfile().subscribe({
      next: (profile) => {
        this.operatorProfile = profile;
        
        if (profile.isApproved) {
          this.loadLayouts();
        } else {
          this.isLoading = false;
          this.errorMessage = this.getApprovalMessage(profile);
        }
      },
      error: (err) => {
        console.error('Failed to load operator profile', err);
        this.errorMessage = 'Failed to load operator profile.';
        this.isLoading = false;
      }
    });
  }

  private getApprovalMessage(profile: OperatorProfileDto): string {
    if (profile.isPending) {
      return 'Your operator account is pending approval. You cannot manage layouts until approved by an administrator.';
    } else if (profile.isRejected) {
      return 'Your operator account has been rejected. Please contact support for more information.';
    } else if (profile.isDisabled) {
      return 'Your operator account has been disabled. Please contact support for more information.';
    }
    return 'Your account status does not allow layout management.';
  }

  private loadLayouts(): void {
    this.operatorService.getLayouts().subscribe({
      next: (data) => {
        this.layouts = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load layouts.';
        this.isLoading = false;
      }
    });
  }

  previewLayout(layout: any): void {
    this.selectedLayout = layout;
    if (layout.seatConfig) {
      this.deckList = layout.seatConfig.decks || ['lower'];
      const maxRow = Math.max(...layout.seatConfig.seats.map((s: any) => s.row), 0);
      this.rowNumbers = Array.from({ length: maxRow }, (_, i) => i + 1);
    }
  }

  seatsForRow(deck: string, row: number): any[] {
    if (!this.selectedLayout?.seatConfig) return [];
    return this.selectedLayout.seatConfig.seats
      .filter((s: any) => s.deck === deck && s.row === row)
      .sort((a: any, b: any) => a.col - b.col);
  }

  deleteLayout(id: string): void {
    if (!confirm('Are you sure you want to delete this layout?')) return;

    this.operatorService.deleteLayout(id).subscribe({
      next: () => {
        this.layouts = this.layouts.filter(l => l.id !== id);
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to delete layout.';
        alert(msg);
      }
    });
  }
}
