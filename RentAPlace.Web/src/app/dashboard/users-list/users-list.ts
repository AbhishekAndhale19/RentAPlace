import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Auth } from '../../services/auth';
import { User } from '../../models/user';
import { Layout } from '../../layout/layout';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, Layout],
  templateUrl: './users-list.html',
  styleUrls: ['./users-list.css']
})
export class UsersList implements OnInit {
  users: User[] = [];
  currentUser!: User; // <-- use definite assignment operator
  isAdmin: boolean = false;
  error: string | null = null;

  constructor(private auth: Auth) {}

  ngOnInit() {
    const storedUser = sessionStorage.getItem('user');
    if (storedUser) {
      this.currentUser = JSON.parse(storedUser);
      this.isAdmin = this.currentUser.role === 'Admin';
    }

    // Fetch users
    this.auth.getAllUsers().subscribe({
      next: (data) => {
        if (this.isAdmin) {
          this.users = data; // Admin sees all
        } else {
          this.users = data.filter(u => u.id === this.currentUser?.id); // Others see self
        }
      },
      error: (err) => this.error = err.error?.message || 'Failed to load users'
    });
  }

  deleteUser(u: User) {
    if (!confirm(`Are you sure you want to delete ${u.fullName}?`)) return;

    this.auth.deleteUser(u.id).subscribe({
      next: () => {
        this.users = this.users.filter(x => x.id !== u.id);
        alert('User deleted successfully.');
      },
      error: (err) => alert(err.error?.message || 'Failed to delete user')
    });
  }

  editUser(u: User) {
    const newName = prompt('Full Name:', u.fullName);
    const newEmail = prompt('Email:', u.email);
    const newRole = this.isAdmin ? prompt('Role (User/Owner/Admin):', u.role) : u.role;

    if (!newName || !newEmail || !newRole) return;

    this.auth.editUser(u.id, { fullName: newName, email: newEmail, role: newRole }).subscribe({
      next: () => {
        Object.assign(u, { fullName: newName, email: newEmail, role: newRole });
        alert('User updated successfully.');

        // Update sessionStorage if editing self
        if (this.currentUser.id === u.id) {
          this.currentUser = u;
          sessionStorage.setItem('user', JSON.stringify(u));
        }
      },
      error: (err) => alert(err.error?.message || 'Failed to update user')
    });
  }
}
