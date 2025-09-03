import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthResponse } from '../models/auth-response';
import { User } from '../models/user';

@Injectable({ providedIn: 'root' })
export class Auth {
  private apiUrl = 'http://localhost:5228/api/auth'; // backend

  constructor(private http: HttpClient) {}

  register(fullName: string, email: string, password: string, isOwner: boolean): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, { fullName, email, password, isOwner });
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password });
  }

  getProfile() {
    // returns user profile from backend using JWT if needed
    return this.http.get<User>('http://localhost:5228/api/users/me');
  }
}
