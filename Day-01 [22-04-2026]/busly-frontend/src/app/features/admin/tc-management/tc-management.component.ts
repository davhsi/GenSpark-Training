import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { environment } from '../../../../environments/environment';
import { TcVersionDto } from '../../../shared/models/admin.models';
import { marked } from 'marked';

interface CreateTcRequest {
  version: string;
  content: string;
  effectiveAt?: string;
}

@Component({
  selector: 'app-tc-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tc-management.component.html',
  styles: [`
    .markdown-preview {
      line-height: 1.6;
    }
    .markdown-preview h1 {
      font-size: 1.5rem;
      font-weight: bold;
      margin: 1rem 0 0.5rem 0;
      color: #2c3e50;
    }
    .markdown-preview h2 {
      font-size: 1.3rem;
      font-weight: bold;
      margin: 0.8rem 0 0.4rem 0;
      color: #34495e;
    }
    .markdown-preview h3 {
      font-size: 1.1rem;
      font-weight: bold;
      margin: 0.6rem 0 0.3rem 0;
      color: #34495e;
    }
    .markdown-preview p {
      margin: 0.5rem 0;
    }
    .markdown-preview ul, .markdown-preview ol {
      margin: 0.5rem 0;
      padding-left: 1.5rem;
    }
    .markdown-preview li {
      margin: 0.2rem 0;
    }
    .markdown-preview a {
      color: #007bff;
      text-decoration: none;
    }
    .markdown-preview a:hover {
      text-decoration: underline;
    }
    .markdown-preview strong {
      font-weight: bold;
    }
    .markdown-preview em {
      font-style: italic;
    }
    .markdown-preview code {
      background-color: #f8f9fa;
      padding: 0.2rem 0.4rem;
      border-radius: 0.25rem;
      font-family: 'Courier New', monospace;
      font-size: 0.9rem;
    }
    .markdown-preview pre {
      background-color: #f8f9fa;
      padding: 1rem;
      border-radius: 0.5rem;
      overflow-x: auto;
      margin: 0.5rem 0;
    }
    .markdown-preview blockquote {
      border-left: 4px solid #007bff;
      padding-left: 1rem;
      margin: 0.5rem 0;
      color: #6c757d;
    }
  `]
})
export class TcManagementComponent implements OnInit {
  versions: TcVersionDto[] = [];
  loading = false;
  submitting = false;
  isPreviewMode = false;
  previewContent: SafeHtml = '';
  showPreviewModal = false;
  previewingVersion: TcVersionDto | null = null;
  isFullscreen = false;
  activeTab: 'publish' | 'history' = 'publish';

  newTc = {
    version: '',
    content: '',
    effectiveAt: ''
  };

  constructor(private http: HttpClient, private sanitizer: DomSanitizer) {}

  ngOnInit(): void {
    this.loadVersions();
    this.generateNextVersion();
  }

  loadVersions(): void {
    this.loading = true;
    this.http.get<TcVersionDto[]>(`${environment.apiUrl}/admin/tc`).subscribe({
      next: (data) => {
        this.versions = data;
        this.loading = false;
        this.generateNextVersion();
      },
      error: (err) => {
        console.error('Failed to load T&C versions', err);
        this.loading = false;
      }
    });
  }

  publishTc(): void {
    if (!this.newTc.version || !this.newTc.content) return;

    this.submitting = true;
    const request: CreateTcRequest = {
      version: this.newTc.version,
      content: this.newTc.content,
      effectiveAt: this.newTc.effectiveAt ? new Date(this.newTc.effectiveAt).toISOString() : undefined
    };
    
    this.http.post(`${environment.apiUrl}/admin/tc`, request).subscribe({
      next: () => {
        this.newTc = { version: '', content: '', effectiveAt: '' };
        this.loadVersions();
        this.submitting = false;
        alert('New T&C version published successfully!');
      },
      error: (err) => {
        console.error('Failed to publish T&C', err);
        this.submitting = false;
        alert('Failed to publish T&C version.');
      }
    });
  }

  generateNextVersion(): void {
    if (this.versions.length === 0) {
      this.newTc.version = 'v1.0';
      return;
    }

    const latestVersion = this.versions[0]; // Assuming versions are ordered by creation date
    const versionMatch = latestVersion.version.match(/v(\d+)\.(\d+)/i);
    
    if (versionMatch) {
      const major = parseInt(versionMatch[1]);
      const minor = parseInt(versionMatch[2]);
      this.newTc.version = `v${major}.${minor + 1}`;
    } else {
      // Fallback if version format doesn't match expected pattern
      this.newTc.version = 'v1.1';
    }
  }

  cloneVersion(version: TcVersionDto): void {
    this.newTc.content = version.content;
    // Keep the auto-generated version
    this.generateNextVersion();
    // Update preview
    this.updatePreview();
    // Switch to publish tab to show the cloned content
    this.activeTab = 'publish';
  }

  togglePreview(): void {
    this.isPreviewMode = !this.isPreviewMode;
    if (this.isPreviewMode) {
      this.updatePreview();
    }
  }

  async updatePreview(): Promise<void> {
    if (this.newTc.content) {
      const html = await marked(this.newTc.content);
      this.previewContent = this.sanitizer.bypassSecurityTrustHtml(html);
    } else {
      this.previewContent = '';
    }
  }

  onContentChange(): void {
    if (this.isPreviewMode) {
      this.updatePreview();
    }
  }

  async previewVersion(version: TcVersionDto): Promise<void> {
    this.previewingVersion = version;
    const html = await marked(version.content);
    this.previewContent = this.sanitizer.bypassSecurityTrustHtml(html);
    this.showPreviewModal = true;
  }

  closePreview(): void {
    this.showPreviewModal = false;
    this.previewingVersion = null;
    this.previewContent = '';
    this.isFullscreen = false;
  }

  toggleFullscreen(): void {
    this.isFullscreen = !this.isFullscreen;
  }

  @HostListener('document:keydown', ['$event'])
  handleEscape(event: KeyboardEvent): void {
    if (event.key === 'Escape' && this.showPreviewModal) {
      this.closePreview();
    }
  }
}
