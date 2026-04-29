import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
Chart.register(...registerables);
import { AdminService } from '../../../core/services/admin.service';
import { ConvenienceFeeConfig, MonthlyRevenueDto, OperatorRevenueDto } from '../../../shared/models/admin.models';

@Component({
  selector: 'app-revenue',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './revenue.component.html'
})
export class RevenueComponent implements OnInit {
  @ViewChild('revenueChart') revenueChartCanvas!: ElementRef<HTMLCanvasElement>;
  chart: Chart | null = null;

  monthlyRevenue: MonthlyRevenueDto[] = [];
  operatorRevenue: OperatorRevenueDto[] = [];
  totalRevenue = 0;
  totalBookingCount = 0;
  isLoading = true;
  errorMessage: string | null = null;

  // Fee config state
  feeConfig: ConvenienceFeeConfig | null = null;
  isFeeLoading = true;
  isEditingFee = false;
  isSavingFee = false;
  feeError: string | null = null;
  feeSaved = false;
  feeForm: FormGroup;

  constructor(private adminService: AdminService, private fb: FormBuilder) {
    this.feeForm = this.fb.group({
      feeType:  ['flat', Validators.required],
      feeValue: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadRevenue();
    this.loadFeeConfig();
  }

  private loadRevenue(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.adminService.getRevenue().subscribe({
      next: monthly => {
        this.monthlyRevenue = monthly;
        this.totalRevenue = monthly.reduce((sum, m) => sum + (m.totalConvenienceFee ?? 0), 0);
        this.updateChart();
      },
      error: () => {
        this.errorMessage = 'Failed to load revenue data.';
        this.isLoading = false;
      }
    });

    this.adminService.getRevenueByOperator().subscribe({
      next: operators => {
        this.operatorRevenue = operators;
        this.totalBookingCount = operators.reduce((sum, o) => sum + (o.bookingCount ?? 0), 0);
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }

  private loadFeeConfig(): void {
    this.isFeeLoading = true;
    this.adminService.getConvenienceFeeConfig().subscribe({
      next: cfg => {
        this.feeConfig = cfg;
        this.isFeeLoading = false;
      },
      error: () => { this.isFeeLoading = false; }
    });
  }

  startEditFee(): void {
    if (!this.feeConfig) return;
    this.feeForm.patchValue({ feeType: this.feeConfig.feeType, feeValue: this.feeConfig.feeValue });
    this.feeError = null;
    this.feeSaved = false;
    this.isEditingFee = true;
  }

  cancelEditFee(): void {
    this.isEditingFee = false;
    this.feeError = null;
  }

  saveFeeConfig(): void {
    if (this.feeForm.invalid) return;
    this.isSavingFee = true;
    this.feeError = null;
    this.feeSaved = false;
    this.adminService.updateConvenienceFeeConfig(this.feeForm.value).subscribe({
      next: () => {
        this.isSavingFee = false;
        this.isEditingFee = false;
        this.feeSaved = true;
        this.feeConfig = this.feeForm.value;
        setTimeout(() => this.feeSaved = false, 3000);
      },
      error: (err) => {
        this.isSavingFee = false;
        this.feeError = err.error?.message || 'Failed to update fee. Please try again.';
      }
    });
  }

  feeLabel(): string {
    if (!this.feeConfig) return '—';
    return this.feeConfig.feeType === 'percent'
      ? `${this.feeConfig.feeValue}% of base fare`
      : `₹${this.feeConfig.feeValue} flat per booking`;
  }

  monthName(month: number): string {
    const months = [
      'January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December'
    ];
    return months[month - 1] ?? String(month);
  }

  private createChart(): void {
    if (this.chart) this.chart.destroy();
    if (!this.revenueChartCanvas) return;
    const ctx = this.revenueChartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    const sortedData = [...this.monthlyRevenue].sort((a, b) =>
      a.year !== b.year ? a.year - b.year : a.month - b.month
    );

    this.chart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: sortedData.map(m => `${this.monthName(m.month)} ${m.year}`),
        datasets: [{
          label: 'Platform Revenue (₹)',
          data: sortedData.map(m => m.totalConvenienceFee),
          backgroundColor: 'rgba(99, 102, 241, 0.6)',
          borderColor: '#6366F1',
          borderWidth: 1,
          borderRadius: 6,
          hoverBackgroundColor: 'rgba(99, 102, 241, 0.85)'
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: '#0F172A',
            borderColor: 'rgba(99,102,241,0.3)',
            borderWidth: 1,
            titleColor: '#ffffff',
            bodyColor: '#94A3B8',
            padding: 12,
            callbacks: {
              label: (ctx) => ' ₹' + (ctx.raw as number).toLocaleString('en-IN', { minimumFractionDigits: 2 })
            }
          }
        },
        scales: {
          x: {
            grid: { color: 'rgba(255,255,255,0.04)' },
            ticks: { color: '#94A3B8', font: { size: 11 } },
            border: { color: 'rgba(255,255,255,0.06)' }
          },
          y: {
            beginAtZero: true,
            grid: { color: 'rgba(255,255,255,0.04)' },
            ticks: { color: '#94A3B8', font: { size: 11 }, callback: (v) => '₹' + v },
            border: { color: 'rgba(255,255,255,0.06)' }
          }
        }
      }
    });
  }

  private updateChart(): void {
    setTimeout(() => this.createChart(), 0);
  }
}
