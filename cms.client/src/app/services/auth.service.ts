import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginRequest } from '../models/login-request';
import { LoginResult } from '../models/login-result';
import { RegisterRequest } from '../models/register-request';
import { UserResponse } from '../models/user-response';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenKey = 'auth_token';
  private userKey = 'auth_user';

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<LoginResult> {
    return this.http.post<LoginResult>('/api/user/login', request).pipe(
      tap(result => {
        if (result.success && result.token && result.user) {
          localStorage.setItem(this.tokenKey, result.token);
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
      tap(() => this.clearStorage())
    );
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getCurrentUser(): UserResponse | null {
    const user = localStorage.getItem(this.userKey);
    return user ? JSON.parse(user) : null;
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const roles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      if (!roles) return false;
      const roleList = Array.isArray(roles) ? roles : [roles];
      return roleList.includes('Admin');
    } catch {
      return false;
    }
  }

  getCurrentUserId(): number | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const id = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
      return id ? parseInt(id, 10) : null;
    } catch {
      return null;
    }
  }

  clearStorage(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
  }
}
