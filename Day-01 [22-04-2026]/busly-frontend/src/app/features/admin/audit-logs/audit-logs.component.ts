import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../../core/services/admin.service';
import { AuditLogDto } from '../../../shared/models/admin.models';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-logs.component.html',
  styles: [`
    .metadata-json {
      font-size: 0.8rem;
      background: #f8f9fa;
      padding: 0.5rem;
      border-radius: 4px;
      max-height: 100px;
      overflow-y: auto;
    }
  `]
})
export class AuditLogsComponent implements OnInit {
  logs: AuditLogDto[] = [];
  filteredLogs: AuditLogDto[] = [];
  isLoading = true;
  errorMessage: string | null = null;
  searchTerm = '';

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading = true;
    this.adminService.getAuditLogs().subscribe({
      next: (data) => {
        this.logs = data;
        this.applyFilter();
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Failed to load audit logs.';
        this.isLoading = false;
      }
    });
  }

  applyFilter(): void {
    if (!this.searchTerm) {
      this.filteredLogs = this.logs;
      return;
    }

    const term = this.searchTerm.toLowerCase();
    this.filteredLogs = this.logs.filter(l => 
      l.action.toLowerCase().includes(term) ||
      l.actorRole.toLowerCase().includes(term) ||
      l.entityType.toLowerCase().includes(term) ||
      (l.metadata && l.metadata.toLowerCase().includes(term))
    );
  }

  formatMetadata(metadata?: string): any {
    if (!metadata) return null;
    try {
      return JSON.parse(metadata);
    } catch {
      return metadata;
    }
  }

  isJson(val: any): boolean {
    return typeof val === 'object' && val !== null;
  }
}
