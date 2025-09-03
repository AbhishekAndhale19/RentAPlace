import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Auth } from '../../services/auth';
import { User } from '../../models/user';
import { Layout } from '../../layout/layout';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule,Layout],
  templateUrl: './users-list.html',
  styleUrls: ['./users-list.css']
})
export class UsersList implements OnInit {
  users: User[] = [];
  error: string | null = null;

  constructor(private auth: Auth) {}

  ngOnInit() {
    this.auth.getAllUsers().subscribe({
      next: (data) => this.users = data,
      error: (err) => this.error = err.error?.message || 'Failed to load users'
    });
  }
}
