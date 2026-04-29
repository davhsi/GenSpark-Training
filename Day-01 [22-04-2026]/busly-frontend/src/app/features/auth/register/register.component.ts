import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { HttpErrorResponse } from '@angular/common/http';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { TcVersionDto } from '../../../shared/models/auth.models';
import { marked } from 'marked';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

export const passwordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');

  return password && confirmPassword && password.value !== confirmPassword.value 
    ? { passwordMismatch: true } 
    : null;
};

type Role = 'Customer' | 'Operator';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
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
export class RegisterComponent {
  selectedRole: Role = 'Customer';
  customerForm: FormGroup;
  operatorForm: FormGroup;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  isLoading = false;
  showPassword = false;
  showConfirmPassword = false;
  
  // T&C Modal properties
  showTcModal = false;
  tcContent: SafeHtml = '';
  tcVersion: string = '';
  tcLoading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private sanitizer: DomSanitizer
  ) {
    this.customerForm = this.fb.group({
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/)]],
      confirmPassword: ['', Validators.required],
      acceptedTerms: [false, Validators.requiredTrue]
    }, { validators: passwordMatchValidator });

    this.operatorForm = this.fb.group({
      companyName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/)]],
      confirmPassword: ['', Validators.required],
      phone: [''],
      acceptedTerms: [false, Validators.requiredTrue]
    }, { validators: passwordMatchValidator });
  }

  selectRole(role: Role): void {
    this.selectedRole = role;
    this.errorMessage = null;
    this.successMessage = null;
  }

  get activeForm(): FormGroup {
    return this.selectedRole === 'Customer' ? this.customerForm : this.operatorForm;
  }

  // Customer field accessors
  get cusUsername() { return this.customerForm.get('username')!; }
  get cusEmail() { return this.customerForm.get('email')!; }
  get cusPassword() { return this.customerForm.get('password')!; }
  get cusConfirmPassword() { return this.customerForm.get('confirmPassword')!; }

  // Operator field accessors
  get opCompanyName() { return this.operatorForm.get('companyName')!; }
  get opEmail() { return this.operatorForm.get('email')!; }
  get opPassword() { return this.operatorForm.get('password')!; }
  get opConfirmPassword() { return this.operatorForm.get('confirmPassword')!; }
  get opPhone() { return this.operatorForm.get('phone')!; }

  onSubmit(): void {
    const form = this.activeForm;
    if (form.invalid) {
      form.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const { confirmPassword, ...registerData } = form.value;

    const request$ = this.selectedRole === 'Customer'
      ? this.authService.registerCustomer(registerData)
      : this.authService.registerOperator(registerData);

    request$.subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigate(['/login'], {
          queryParams: { 
            registered: 'true',
            email: this.activeForm.value.email
          }
        });
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
        if (err.status === 409) {
          this.errorMessage = 'Email already registered';
        } else {
          this.errorMessage = 'Registration failed. Please try again.';
        }
      }
    });
  }

  // T&C Modal methods
  showTermsAndConditions(): void {
    this.tcLoading = true;
    this.showTcModal = true;
    
    this.authService.getCurrentTc().subscribe({
      next: async (tc: TcVersionDto) => {
        this.tcVersion = tc.version;
        const html = await marked(tc.content);
        this.tcContent = this.sanitizer.bypassSecurityTrustHtml(html);
        this.tcLoading = false;
      },
      error: () => {
        this.tcContent = this.sanitizer.bypassSecurityTrustHtml('<p class="text-danger">Unable to load Terms & Conditions. Please try again later.</p>');
        this.tcLoading = false;
      }
    });
  }

  closeTcModal(): void {
    this.showTcModal = false;
    this.tcContent = '';
    this.tcVersion = '';
  }
}
