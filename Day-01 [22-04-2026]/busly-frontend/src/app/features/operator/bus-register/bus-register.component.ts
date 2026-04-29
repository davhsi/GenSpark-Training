import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { OperatorService } from '../../../core/services/operator.service';
import { AdminService } from '../../../core/services/admin.service';
import { RouteDto } from '../../../shared/models/admin.models';
import { LayoutDto } from '../../../shared/models/operator.models';

@Component({
  selector: 'app-bus-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './bus-register.component.html'
})
export class BusRegisterComponent implements OnInit {
  form: FormGroup;
  routes: RouteDto[] = [];
  layouts: LayoutDto[] = [];
  errorMessage = '';
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private operatorService: OperatorService,
    private adminService: AdminService,
    private router: Router
  ) {
    this.form = this.fb.group({
      busNumber:      ['', Validators.required],
      busName:        ['', Validators.required],
      ownerName:      ['', Validators.required],
      driverName:     [''],
      driverPhone:    [''],
      conductorName:  [''],
      conductorPhone: [''],
      basePrice:      [null, [Validators.required, Validators.min(0)]],
      routeId:        ['', Validators.required],
      layoutId:       ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.adminService.getActiveRoutes().subscribe({
      next: (data) => (this.routes = data),
      error: () => (this.errorMessage = 'Failed to load routes.')
    });

    this.operatorService.getLayouts().subscribe({
      next: (data) => {
        this.layouts = data;
        // Restore form data if returning from layout creation
        this.restoreFormData();
      },
      error: () => (this.errorMessage = 'Failed to load layouts.')
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const payload = {
      ...this.form.value,
      basePrice: Math.round(Number(this.form.value.basePrice) * 100) / 100
    };

    this.operatorService.registerBus(payload).subscribe({
      next: () => this.router.navigate(['/operator/buses']),
      error: () => {
        this.errorMessage = 'Failed to register bus. Please try again.';
        this.isSubmitting = false;
      }
    });
  }

  isInvalid(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }

  goToCreateLayout(): void {
    // Store the current form data and navigation state
    sessionStorage.setItem('busRegistrationForm', JSON.stringify(this.form.value));
    sessionStorage.setItem('returnToBusRegistration', 'true');
    
    // Navigate to layout builder
    this.router.navigate(['/operator/layouts/new']);
  }

  restoreFormData(): void {
    const savedForm = sessionStorage.getItem('busRegistrationForm');
    if (savedForm) {
      try {
        const formData = JSON.parse(savedForm);
        this.form.patchValue(formData);
        // Clear the saved form data after restoration
        sessionStorage.removeItem('busRegistrationForm');
      } catch (error) {
        console.warn('Failed to restore form data:', error);
        sessionStorage.removeItem('busRegistrationForm');
      }
    }
  }
}
