import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule,Router } from '@angular/router';
import { Auth } from '../../services/auth';


@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, CommonModule,RouterModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class Register {
  fullName = '';
  email = '';
  password = '';
  isOwner = false;
  error = '';
  success = ''; // <-- added success property
  showPassword = false;

  constructor(
    private auth: Auth,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: any
  ) {}

  onRegister(form: NgForm) {
  if (form.invalid) {
    Object.values(form.controls).forEach((control: any) => control.markAsTouched());
    return;
  }

  this.auth.register(this.fullName, this.email, this.password, this.isOwner).subscribe({
    next: () => {
      alert('Registration successful! Please login now.');
      this.router.navigate(['/login']);
    },
    error: () => {
      alert('Registration failed. Try again.');
    }
  });
}

}
