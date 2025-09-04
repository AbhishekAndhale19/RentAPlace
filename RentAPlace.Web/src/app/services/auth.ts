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

  //forgot password and reset, change password

  forgotPassword(email:string):Observable<any>{
    return this.http.post(`${this.apiUrl}/forgot-password`,{email});
  }

  resetPassword(token: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, { token, newPassword });
  }

  changePassword(oldPassword:string,newPassword:string):Observable<any>{
    const token=sessionStorage.getItem('token');
    return this.http.patch(`http://localhost:5228/api/users/change-password`,{oldPassword,newPassword},{
      headers:{Authorization:`Bearer ${token}`}
    });
  }

  getAllUsers(): Observable<User[]> {
  const token = sessionStorage.getItem('token');
  return this.http.get<User[]>(
    'http://localhost:5228/api/users',
    {
      headers: { Authorization: `Bearer ${token}` }
    }
  );
}

}
