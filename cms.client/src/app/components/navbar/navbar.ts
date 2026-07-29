import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css'],
  standalone: false
})
export class Navbar {
  constructor(private auth: AuthService, private router: Router) {}

  get isAuthenticated(): boolean { return this.auth.isAuthenticated(); }
  get isAdmin(): boolean { return this.auth.isAdmin(); }
  get currentUserName(): string | null {
    const user = this.auth.getCurrentUser();
    return user ? (user.displayName || user.userName) : null;
  }

  logout(): void {
    this.auth.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => this.router.navigate(['/login'])
    });
  }
}
