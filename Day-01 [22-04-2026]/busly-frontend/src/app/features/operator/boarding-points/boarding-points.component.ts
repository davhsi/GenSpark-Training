import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { OperatorService } from '../../../core/services/operator.service';
import { BusStopDto } from '../../../shared/models/operator.models';

@Component({
  selector: 'app-boarding-points',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './boarding-points.component.html'
})
export class BoardingPointsComponent implements OnInit {
  busId = '';
  busName = '';
  sourceCity = '';
  destinationCity = '';
  boardingForm: FormGroup;
  droppingForm: FormGroup;

  boardingStops: BusStopDto[] = [];
  droppingStops: BusStopDto[] = [];

  boardingSuccess = '';
  boardingError = '';
  droppingSuccess = '';
  droppingError = '';

  isAddingBoarding = false;
  isAddingDropping = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private operatorService: OperatorService
  ) {
    this.boardingForm = this.fb.group({
      city:          ['', Validators.required],
      address:       ['', Validators.required],
      scheduledTime: ['', [Validators.required, Validators.pattern(/^\d{2}:\d{2}$/)]]
    });

    this.droppingForm = this.fb.group({
      city:          ['', Validators.required],
      address:       ['', Validators.required],
      scheduledTime: ['', [Validators.required, Validators.pattern(/^\d{2}:\d{2}$/)]]
    });
  }

  ngOnInit(): void {
    this.busId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadBusDetails();
  }

  loadBusDetails(): void {
    this.operatorService.getBusById(this.busId).subscribe({
      next: (bus) => {
        this.busName = bus.busName || 'Bus';
        this.sourceCity = bus.sourceCity || '';
        this.destinationCity = bus.destinationCity || '';
        this.boardingStops = bus.stops?.filter(s => s.type === 'BOARDING') || [];
        this.droppingStops = bus.stops?.filter(s => s.type === 'DROPPING') || [];
        
        // Prefill forms with source and destination cities
        this.prefillCityFields();
      }
    });
  }

  addBoarding(): void {
    if (this.boardingForm.invalid) {
      this.boardingForm.markAllAsTouched();
      return;
    }

    this.isAddingBoarding = true;
    this.boardingSuccess = '';
    this.boardingError = '';

    const request = { ...this.boardingForm.value, type: 'BOARDING' };

    this.operatorService.addBoardingPoint(this.busId, request).subscribe({
      next: () => {
        this.boardingSuccess = 'Boarding point added successfully!';
        this.boardingForm.reset();
        this.isAddingBoarding = false;
        this.loadBusDetails();
      },
      error: () => {
        this.boardingError = 'Failed to add boarding point.';
        this.isAddingBoarding = false;
      }
    });
  }

  addDropping(): void {
    if (this.droppingForm.invalid) {
      this.droppingForm.markAllAsTouched();
      return;
    }

    this.isAddingDropping = true;
    this.droppingSuccess = '';
    this.droppingError = '';

    const request = { ...this.droppingForm.value, type: 'DROPPING' };

    this.operatorService.addDroppingPoint(this.busId, request).subscribe({
      next: () => {
        this.droppingSuccess = 'Dropping point added successfully!';
        this.droppingForm.reset();
        this.isAddingDropping = false;
        this.loadBusDetails();
      },
      error: () => {
        this.droppingError = 'Failed to add dropping point.';
        this.isAddingDropping = false;
      }
    });
  }

  removeStop(stopId: string): void {
    if (!confirm('Remove this stop?')) return;

    this.operatorService.removeBusStop(stopId).subscribe({
      next: () => this.loadBusDetails(),
      error: () => alert('Failed to remove stop.')
    });
  }

  isBoardingInvalid(field: string): boolean {
    const ctrl = this.boardingForm.get(field);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }

  isDroppingInvalid(field: string): boolean {
    const ctrl = this.droppingForm.get(field);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }

  prefillCityFields(): void {
    // Prefill boarding form with source city if form is empty
    if (this.sourceCity && !this.boardingForm.get('city')?.value) {
      this.boardingForm.patchValue({ city: this.sourceCity });
    }
    
    // Prefill dropping form with destination city if form is empty
    if (this.destinationCity && !this.droppingForm.get('city')?.value) {
      this.droppingForm.patchValue({ city: this.destinationCity });
    }
  }
}
