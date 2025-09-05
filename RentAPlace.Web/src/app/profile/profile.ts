import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Auth } from '../services/auth';
import { User } from '../models/user';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.html',
  styleUrls: ['./profile.css']
})
export class Profile implements OnInit {
  user: User = { id: '', fullName: '', email: '', role: 'User' } as any;

  constructor(private auth: Auth, public router: Router, @Inject(PLATFORM_ID) private platformId: any) {}

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      const storedUser = sessionStorage.getItem('user');
      if (storedUser) this.user = JSON.parse(storedUser);
      else this.router.navigate(['/login']);
    }
  }

  updateProfile(form: NgForm) {
  if (form.invalid) {
    Object.values(form.controls).forEach(c => c.markAsTouched());
    return;
  }

  this.auth.updateProfile({
    fullName: this.user.fullName,
    email: this.user.email
  }).subscribe({
    next: (updatedUser) => {
      if (isPlatformBrowser(this.platformId)) {
        let currentUser: any = {};
        const stored = sessionStorage.getItem('user');
        if (stored) currentUser = JSON.parse(stored);

        const mergedUser = {
          ...currentUser,
          ...updatedUser
        };

        sessionStorage.setItem('user', JSON.stringify(mergedUser));
      }
      alert('Profile updated successfully!');
      this.router.navigate(['/dashboard']);
    },
    error: (err) => alert(err.error?.message || 'Failed to update profile')
  });
}
}
