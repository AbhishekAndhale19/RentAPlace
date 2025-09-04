import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule,Router } from '@angular/router';
import { Auth } from '../services/auth';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterModule],
  templateUrl: './forgot-password.html'
})
export class ForgotPassword {
  email = '';
  message = '';
  error = '';

  constructor(private auth: Auth,private router:Router) {}

  onSubmit(form: NgForm) {
  if (form.invalid) return;
  this.auth.forgotPassword(this.email).subscribe({
    next: (res: any) => {
      this.message = res.message;
      this.error = '';
      this.router.navigate(['/reset-password'], { queryParams: { token: res.token } });
    },
    error: () => {
      this.error = 'Failed to send reset link';
      this.message = '';
    }
  });
}

}
