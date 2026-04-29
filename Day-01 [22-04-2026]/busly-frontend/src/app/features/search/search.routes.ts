import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { ResultsComponent } from './results/results.component';
import { SeatSelectionComponent } from './seat-selection/seat-selection.component';

export const searchRoutes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'results', component: ResultsComponent },
  { path: 'seats/:busId', component: SeatSelectionComponent }
];
