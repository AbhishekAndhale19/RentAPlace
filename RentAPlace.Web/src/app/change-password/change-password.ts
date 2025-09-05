import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Auth } from '../services/auth';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './change-password.html',
  styleUrls: ['./change-password.css']
})
export class ChangePassword {
  oldPassword = '';
  newPassword = '';
  showOld = false;
  showNew = false;

  constructor(
    private auth: Auth,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: any
  ) {}

  onSubmit(form: NgForm) {
    if (form.invalid) {
      Object.values(form.controls).forEach(c => c.markAsTouched());
      return;
    }

    this.auth.changePassword(this.oldPassword, this.newPassword).subscribe({
      next: () => {
        alert('Password changed successfully!');
        // redirect to dashboard
        this.router.navigate(['/dashboard']);
      },
      error: (err) =>
        alert(err.error?.message || 'Failed to change password')
    });
  }
}
