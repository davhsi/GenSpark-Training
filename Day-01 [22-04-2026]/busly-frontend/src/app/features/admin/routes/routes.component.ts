import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { AdminService } from '../../../core/services/admin.service';
import { RouteDto } from '../../../shared/models/admin.models';

@Component({
  selector: 'app-routes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './routes.component.html'
})
export class RoutesComponent implements OnInit {
  routes: RouteDto[] = [];
  routeForm: FormGroup;
  errorMessage: string | null = null;
  isSubmitting = false;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder
  ) {
    this.routeForm = this.fb.group({
      sourceCity: ['', Validators.required],
      destinationCity: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadRoutes();
  }

  get sourceCity() {
    return this.routeForm.get('sourceCity')!;
  }

  get destinationCity() {
    return this.routeForm.get('destinationCity')!;
  }

  loadRoutes(): void {
    this.adminService.getAllRoutes().subscribe({
      next: (data) => (this.routes = data),
      error: () => (this.errorMessage = 'Failed to load routes.')
    });
  }

  createRoute(): void {
    if (this.routeForm.invalid) {
      this.routeForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = null;

    this.adminService.createRoute(this.routeForm.value).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.routeForm.reset();
        this.loadRoutes();
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        if (err.status === 409) {
          this.errorMessage = 'A route between these cities already exists.';
        } else {
          this.errorMessage = 'Failed to create route. Please try again.';
        }
      }
    });
  }

  toggleRoute(id: string): void {
    this.adminService.toggleRoute(id).subscribe({
      next: () => this.loadRoutes(),
      error: () => (this.errorMessage = 'Failed to toggle route status.')
    });
  }
}
