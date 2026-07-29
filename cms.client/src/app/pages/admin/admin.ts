import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { UserResponse } from '../../models/user-response';
import { UpdateUserRequest } from '../../models/update-user-request';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.html',
  standalone: false
})
export class Admin implements OnInit {
  users: UserResponse[] = [];
  filteredUsers: UserResponse[] = [];
  search = '';
  loading = false;
  error = '';
  message = '';

  editingUser: UserResponse | null = null;
  editModel: UpdateUserRequest = {};
  newRole = '';

  constructor(private userService: UserService, public auth: AuthService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.userService.getAll().subscribe({
      next: data => {
        this.users = data;
        this.users.forEach(u => this.fetchRoles(u));
        this.applyFilter();
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load users.';
        this.loading = false;
      }
    });
  }

  fetchRoles(user: UserResponse): void {
    this.userService.getRoles(user.id).subscribe({
      next: roles => user.roles = roles
    });
  }

  applyFilter(): void {
    const q = this.search.toLowerCase();
    this.filteredUsers = this.users.filter(u =>
      u.userName.toLowerCase().includes(q) ||
      u.displayName.toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q)
    );
  }

  startEdit(user: UserResponse): void {
    this.editingUser = user;
    this.editModel = { userName: user.userName, email: user.email, displayName: user.displayName };
    this.error = '';
    this.message = '';
  }

  cancelEdit(): void {
    this.editingUser = null;
    this.editModel = {};
  }

  saveEdit(): void {
    if (!this.editingUser) return;
    this.loading = true;
    this.userService.update(this.editingUser.id, this.editModel).subscribe({
      next: updated => {
        const idx = this.users.findIndex(u => u.id === updated.id);
        if (idx > -1) this.users[idx] = updated;
        this.applyFilter();
        this.cancelEdit();
        this.message = 'User updated successfully.';
        this.loading = false;
      },
      error: err => {
        this.error = err.error?.error ?? 'Update failed.';
        this.loading = false;
      }
    });
  }

  confirmDelete(user: UserResponse): void {
    if (!confirm(`Delete user "${user.userName}" (ID: ${user.id})? This cannot be undone.`)) return;
    this.loading = true;
    this.userService.delete(user.id).subscribe({
      next: () => {
        this.users = this.users.filter(u => u.id !== user.id);
        this.applyFilter();
        this.message = `User "${user.userName}" deleted.`;
        this.loading = false;
      },
      error: err => {
        this.error = err.error?.error ?? 'Delete failed.';
        this.loading = false;
      }
    });
  }

  assignRole(user: UserResponse): void {
    if (!this.newRole.trim()) return;
    this.userService.assignRole(user.id, this.newRole.trim()).subscribe({
      next: () => {
        this.fetchRoles(user);
        this.message = `Role "${this.newRole.trim()}" assigned to ${user.userName}.`;
        this.newRole = '';
      },
      error: err => {
        this.error = err.error?.error ?? 'Failed to assign role.';
      }
    });
  }
}
