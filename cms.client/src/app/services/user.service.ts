import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserResponse } from '../models/user-response';
import { UpdateUserRequest } from '../models/update-user-request';

@Injectable({ providedIn: 'root' })
export class UserService {
  private baseUrl = '/api/user';

  constructor(private http: HttpClient) {}

  getAll(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(this.baseUrl);
  }

  getById(id: number): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.baseUrl}/${id}`);
  }

  update(id: number, request: UpdateUserRequest): Observable<UserResponse> {
    return this.http.put<UserResponse>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  assignRole(id: number, role: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${id}/roles`, `"${role}"`, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  getRoles(id: number): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/${id}/roles`);
  }
}
