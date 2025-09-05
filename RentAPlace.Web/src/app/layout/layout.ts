import { Component, Input, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';

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

  ngOnInit(): void {
  // Load user when layout starts
  this.loadUser();

  // Reload user on every navigation
  this.router.events.subscribe(event => {
    if (event instanceof NavigationEnd) {
      this.loadUser();
    }
  });
}


  loadUser() {
    if (isPlatformBrowser(this.platformId)) {
      const storedUser = sessionStorage.getItem('user');
      if (storedUser) {
        this.user = JSON.parse(storedUser);
      } else {
        this.user = null;
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
