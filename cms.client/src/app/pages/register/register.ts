import { Component, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RegisterRequest } from '../../models/register-request';

@Component({
  selector: 'app-register',
  templateUrl: './register.html',
  standalone: false
})
export class Register {
  model: RegisterRequest = { userName: '', email: '', password: '', displayName: '' };
  confirmPassword = '';
  errors: string[] = [];
  userNameErrors: string[] = [];
  emailErrors: string[] = [];
  displayNameErrors: string[] = [];
  passwordErrors: string[] = [];
  loading = false;
  suggestedUserName = '';

  constructor(private auth: AuthService, private router: Router, private cdr: ChangeDetectorRef) {}

  applySuggestion(): void {
    this.model.userName = this.suggestedUserName;
    this.suggestedUserName = '';
  }

  private categorizeErrors(messages: string[]): void {
    this.errors = [];
    this.userNameErrors = [];
    this.emailErrors = [];
    this.displayNameErrors = [];
    this.passwordErrors = [];
    for (const msg of messages) {
      const lower = msg.toLowerCase();
      switch (true) {
        case lower.includes('email'):
          this.emailErrors.push(msg); break;
        case lower.includes('display name'):
          this.displayNameErrors.push(msg); break;
        case lower.includes('username') || lower.includes('taken'):
          this.userNameErrors.push(msg); break;
        case lower.includes('password'):
          this.passwordErrors.push(msg); break;
        default:
          this.errors.push(msg);
      }
    }
  }

  submit(): void {
    this.errors = [];
    this.userNameErrors = [];
    this.emailErrors = [];
    this.displayNameErrors = [];
    this.passwordErrors = [];
    this.suggestedUserName = '';
    if (this.model.password !== this.confirmPassword) {
      this.passwordErrors = ['Passwords do not match.'];
      return;
    }
    this.loading = true;
    this.cdr.detectChanges();
    this.auth.register(this.model).subscribe({
      next: () => {
        this.loading = false;
        this.cdr.detectChanges();
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.loading = false;
        const data = err.error;
        this.categorizeErrors(data?.errors ?? ['Registration failed.']);
        if (data?.suggestedUserName) {
          this.suggestedUserName = data.suggestedUserName;
        }
        this.cdr.detectChanges();
      }
    });
  }
}


