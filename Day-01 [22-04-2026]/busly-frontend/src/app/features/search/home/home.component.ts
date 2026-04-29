import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { SearchService } from '../../../core/services/search.service';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  searchForm: FormGroup;
  fromSuggestions: string[] = [];
  toSuggestions: string[] = [];
  minDate: string;
  maxDate: string;

  private fromInput$ = new Subject<string>();
  private toInput$ = new Subject<string>();

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private searchService: SearchService
  ) {
    // Set date constraints
    const today = new Date();
    const maxDate = new Date();
    maxDate.setDate(today.getDate() + 90);
    
    this.minDate = today.toISOString().split('T')[0]; // Format as YYYY-MM-DD
    this.maxDate = maxDate.toISOString().split('T')[0]; // Format as YYYY-MM-DD
    
    this.searchForm = this.fb.group({
      from: ['', Validators.required],
      to: ['', Validators.required],
      date: ['', [Validators.required, this.dateValidator.bind(this)]]
    });
  }

  ngOnInit(): void {
    this.fromInput$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => query.length >= 2 ? this.searchService.getCitySuggestions(query) : [[]])
    ).subscribe(cities => this.fromSuggestions = cities);

    this.toInput$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => query.length >= 2 ? this.searchService.getCitySuggestions(query) : [[]])
    ).subscribe(cities => this.toSuggestions = cities);
  }

  get from() { return this.searchForm.get('from')!; }
  get to() { return this.searchForm.get('to')!; }
  get date() { return this.searchForm.get('date')!; }

  private dateValidator(control: any): { [key: string]: any } | null {
    if (!control.value) return null;
    
    const selectedDate = new Date(control.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0); // Set to start of day
    
    const maxDate = new Date();
    maxDate.setDate(today.getDate() + 90); // Max 90 days in advance
    
    // Check if date is in the past
    if (selectedDate < today) {
      return { pastDate: { value: control.value } };
    }
    
    // Check if date is too far in advance
    if (selectedDate > maxDate) {
      return { tooFarFuture: { value: control.value } };
    }
    
    return null;
  }

  onFromInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.fromInput$.next(value);
  }

  onToInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.toInput$.next(value);
  }

  selectFrom(city: string): void {
    this.searchForm.patchValue({ from: city });
    this.fromSuggestions = [];
  }

  selectTo(city: string): void {
    this.searchForm.patchValue({ to: city });
    this.toSuggestions = [];
  }

  onSubmit(): void {
    if (this.searchForm.invalid) {
      this.searchForm.markAllAsTouched();
      return;
    }
    const { from, to, date } = this.searchForm.value;
    this.router.navigate(['/search/results'], {
      queryParams: { from, to, date }
    });
  }
}
