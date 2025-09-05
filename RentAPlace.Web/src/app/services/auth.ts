import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthResponse } from '../models/auth-response';
import { User } from '../models/user';

@Injectable({ providedIn: 'root' })
export class Auth {
  private apiUrl = 'http://localhost:5228/api/auth'; // backend
  private userUrl = 'http://localhost:5228/api/users';

  constructor(private http: HttpClient) {}

  register(fullName: string, email: string, password: string, isOwner: boolean): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, { fullName, email, password, isOwner });
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password });
  }

  getProfile() {
    const token = sessionStorage.getItem('token');
    return this.http.get<User>(`${this.userUrl}/me`, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }

  // forgot password, reset, change password
  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(token: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, { token, newPassword });
  }

  changePassword(oldPassword: string, newPassword: string): Observable<any> {
    const token = sessionStorage.getItem('token');
    return this.http.patch(`${this.userUrl}/change-password`, { oldPassword, newPassword }, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }

  // Admin only
  deleteUser(id: string) {
    const token = sessionStorage.getItem('token');
    return this.http.delete(`${this.userUrl}/${id}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }

  editUser(id: string, data: { fullName: string; email: string; role: string }) {
    const token = sessionStorage.getItem('token');
    return this.http.patch(`${this.userUrl}/${id}`, data, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }

  getAllUsers(): Observable<User[]> {
    const token = sessionStorage.getItem('token');
    return this.http.get<User[]>(this.userUrl, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }

  // 👇 NEW: self update (Profile page)
  updateProfile(data: { fullName: string; email: string }) {
    const token = sessionStorage.getItem('token');
    return this.http.patch<User>(`${this.userUrl}/me`, data, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }
}
