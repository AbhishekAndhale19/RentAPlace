import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Auth } from '../services/auth';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterModule],
  templateUrl: './change-password.html'
})
export class ChangePassword {
  oldPassword = '';
  newPassword = '';
  message = '';
  error = '';
  showOld=false;
  showNew=false;

  constructor(private auth: Auth) {}

  onSubmit(form: NgForm) {
    if (form.invalid) return;
    this.auth.changePassword(this.oldPassword, this.newPassword).subscribe({
      next: (res: any) => {
        this.message = res.message;
        this.error = '';
      },
      error: () => {
        this.error = 'Change password failed';
        this.message = '';
      }
    });
  }
}
