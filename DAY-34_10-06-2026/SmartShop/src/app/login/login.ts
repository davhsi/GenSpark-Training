import { Component, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { form, required, minLength, FormField } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { LoginCredentials } from '../models/user.model';

@Component({
  selector: 'app-login',
  imports: [FormsModule, ReactiveFormsModule, FormField],
  templateUrl: './login.html',
})
export class Login {
  credentials = signal(new LoginCredentials());
  progress = signal(false);
  errorMessage = signal('');

  loginForm = form(this.credentials, (f) => {
    required(f.username, { message: 'Username is required' });
    minLength(f.username, 3, { message: 'Username must be at least 3 characters' });
    required(f.password, { message: 'Password is required' });
    minLength(f.password, 4, { message: 'Password must be at least 4 characters' });
  });

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}
  
  handleLogin() {
    console.log(this.loginForm());
      console.log(this.loginForm().invalid());

    if (this.loginForm().invalid()) {
      return;
    }

    this.progress.set(true);
    this.errorMessage.set('');

    this.authService.login(this.credentials()).subscribe({
      next: (user) => {
        this.progress.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.progress.set(false);
        this.errorMessage.set('Invalid username or password. Please try again.');
      },
    });
  }
}
