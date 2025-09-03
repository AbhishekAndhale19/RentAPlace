import { Component, Input, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './layout.html',
  styleUrls: ['./layout.css']
})
export class Layout implements OnInit {
  @Input() user: any = null;

  constructor(private router: Router, @Inject(PLATFORM_ID) private platformId: any) {}

  ngOnInit() {
    // load user from sessionStorage if not passed
    if (!this.user && isPlatformBrowser(this.platformId)) {
      const storedUser = sessionStorage.getItem('user');
      if (storedUser) {
        this.user = JSON.parse(storedUser);
      }
    }
  }

  logout() {
    if (isPlatformBrowser(this.platformId)) {
      sessionStorage.removeItem('token');
      sessionStorage.removeItem('user');
    }
    this.router.navigate(['/login']);
  }
}
