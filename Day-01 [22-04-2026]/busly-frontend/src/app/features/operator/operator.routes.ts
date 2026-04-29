import { Routes } from '@angular/router';
import { OperatorLayoutComponent } from './operator-layout/operator-layout.component';
import { BusListComponent } from './bus-list/bus-list.component';
import { BusRegisterComponent } from './bus-register/bus-register.component';
import { BoardingPointsComponent } from './boarding-points/boarding-points.component';
import { LayoutBuilderComponent } from './layout-builder/layout-builder.component';
import { LayoutListComponent } from './layout-list/layout-list.component';
import { DashboardComponent } from './dashboard/dashboard.component';

export const operatorRoutes: Routes = [
  {
    path: '',
    component: OperatorLayoutComponent,
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'buses', component: BusListComponent },
      { path: 'buses/register', component: BusRegisterComponent },
      { path: 'buses/:id/stops', component: BoardingPointsComponent },
      { path: 'layouts', component: LayoutListComponent },
      { path: 'layouts/new', component: LayoutBuilderComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  }
];
