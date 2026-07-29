import { Component, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../models/login-request';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  standalone: false
})
export class Login {
  model: LoginRequest = { userName: '', password: '' };
  errors: string[] = [];
  loading = false;

  constructor(private auth: AuthService, private router: Router, private cdr: ChangeDetectorRef) {}

  submit(): void {
    this.errors = [];
    this.loading = true;
    this.cdr.detectChanges();
    this.auth.login(this.model).subscribe({
      next: (result) => {
        this.loading = false;
        if (result.success) {
          this.cdr.detectChanges();
          this.router.navigate(['/users']);
        } else {
          this.errors = result.errors;
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        this.loading = false;
        this.errors = err.error?.errors ?? ['Login failed.'];
        this.cdr.detectChanges();
      }
    });
  }
}
