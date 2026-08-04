import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, finalize, tap } from 'rxjs';
import { LoginRequest } from '../models/login-request';
import { LoginResult } from '../models/login-result';
import { RegisterRequest } from '../models/register-request';
import { UserResponse } from '../models/user-response';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private userKey = 'auth_user';

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<LoginResult> {
    return this.http.post<LoginResult>('/api/user/login', request).pipe(
      tap(result => {
        if (result.success && result.user) {
          localStorage.setItem(this.userKey, JSON.stringify(result.user));
        }
      })
    );
  }

  register(request: RegisterRequest): Observable<UserResponse> {
    return this.http.post<UserResponse>('/api/user/register', request);
  }

  logout(): Observable<any> {
    return this.http.post('/api/user/logout', {}).pipe(
      finalize(() => this.clearStorage())
    );
  }

  getCurrentUser(): UserResponse | null {
    const user = localStorage.getItem(this.userKey);
    return user ? JSON.parse(user) : null;
  }

  isAuthenticated(): boolean {
    return !!this.getCurrentUser();
  }

  isAdmin(): boolean {
    return this.getCurrentUser()?.roles.includes('Admin') ?? false;
  }

  getCurrentUserId(): number | null {
    return this.getCurrentUser()?.id ?? null;
  }

  clearStorage(): void {
    localStorage.removeItem(this.userKey);
  }
}
