import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { Auth } from '../services/auth';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterModule],
  templateUrl: './reset-password.html'
})
export class ResetPassword implements OnInit {
  token = '';
  newPassword = '';
  message = '';
  error = '';
  showNew = false;

  constructor(
    private auth: Auth,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    // Read token from query params
    this.route.queryParams.subscribe(params => {
      this.token = params['token'] || '';
    });
  }

  onSubmit(form: NgForm) {
    if (form.invalid) return;

    this.auth.resetPassword(this.token, this.newPassword).subscribe({
      next: (res: any) => {
        this.message = res.message;
        this.error = '';
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: () => {
        this.error = 'Reset failed. Invalid or expired token';
        this.message = '';
      }
    });
  }
}
