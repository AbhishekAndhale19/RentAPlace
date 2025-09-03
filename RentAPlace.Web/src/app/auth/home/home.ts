import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home {
  constructor(private router: Router, @Inject(PLATFORM_ID) private platformId: Object) {
    // If already logged in we can redirect to dashboard
    if (isPlatformBrowser(this.platformId)) {
      const token = sessionStorage.getItem('token');
      if (token) this.router.navigate(['/dashboard']);
    }
  }

  goToLogin() { this.router.navigate(['/login']); }
  goToRegister() { this.router.navigate(['/register']); }
}
