import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import { RoutesComponent } from './routes/routes.component';
import { OperatorQueueComponent } from './operator-queue/operator-queue.component';
import { BusQueueComponent } from './bus-queue/bus-queue.component';
import { RevenueComponent } from './revenue/revenue.component';
import { TcManagementComponent } from './tc-management/tc-management.component';
import { AuditLogsComponent } from './audit-logs/audit-logs.component';

export const adminRoutes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    children: [
      { path: 'routes', component: RoutesComponent },
      { path: 'operators', component: OperatorQueueComponent },
      { path: 'buses', component: BusQueueComponent },
      { path: 'revenue', component: RevenueComponent },
      { path: 'tc', component: TcManagementComponent },
      { path: 'audit', component: AuditLogsComponent },
      { path: '', redirectTo: 'routes', pathMatch: 'full' }
    ]
  }
];
