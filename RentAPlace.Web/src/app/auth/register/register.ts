import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Auth } from '../../services/auth';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  templateUrl: './register.html',
  styleUrls: ['./register.css'],
  imports: [FormsModule, CommonModule]
})
export class Register {
  fullName = '';
  email = '';
  password = '';
  isOwner = false;
  error = '';

  constructor(private auth: Auth, private router: Router) {}

  onRegister(form: any) {
  if (form.invalid) {
    // mark all fields as touched to show validation
    Object.values(form.controls).forEach((control: any) => control.markAsTouched());
    return;
  }

  this.auth.register(this.fullName, this.email, this.password, this.isOwner).subscribe({
    next: (res) => {
      localStorage.setItem('token', res.accessToken);
      this.router.navigate(['/dashboard']);
    },
    error: () => {
      this.error = 'Registration failed';
    }
  });
}

}
