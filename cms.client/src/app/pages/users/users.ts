import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { UserService } from '../../services/user.service';
import { UserResponse } from '../../models/user-response';

@Component({
  selector: 'app-users',
  templateUrl: './users.html',
  standalone: false
})
export class Users implements OnInit {
  users: UserResponse[] = [];
  filteredUsers: UserResponse[] = [];
  search = '';
  loading = false;
  error = '';

  constructor(private userService: UserService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.cdr.detectChanges();
    this.userService.getAll().subscribe({
      next: data => {
        this.users = data;
        this.applyFilter();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Failed to load users.';
        this.loading = false;
        this.cdr.detectChanges();
      }
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
}
