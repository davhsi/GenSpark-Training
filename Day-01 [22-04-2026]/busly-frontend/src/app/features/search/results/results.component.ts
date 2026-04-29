import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { SearchService } from '../../../core/services/search.service';
import { BusSearchResultDto } from '../../../shared/models/search.models';

@Component({
  selector: 'app-results',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './results.component.html'
})
export class ResultsComponent implements OnInit {
  buses: BusSearchResultDto[] = [];
  from: string = '';
  to: string = '';
  date: string = '';
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private searchService: SearchService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.from = params['from'] ?? '';
      this.to = params['to'] ?? '';
      this.date = params['date'] ?? '';

      if (this.from && this.to && this.date) {
        this.loadBuses();
      }
    });
  }

  private loadBuses(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.searchService.searchBuses(this.from, this.to, this.date).subscribe({
      next: (buses) => {
        this.buses = buses;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load buses. Please try again.';
        this.isLoading = false;
      }
    });
  }

  selectBus(bus: BusSearchResultDto): void {
    this.router.navigate(['/search/seats', bus.busId], {
      queryParams: { date: this.date }
    });
  }
}
