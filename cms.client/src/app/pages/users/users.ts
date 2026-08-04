import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { UserService } from '../../services/user.service';
import { UserResponse } from '../../models/user-response';
import { getInitials } from '../../utils/text.util';

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
  pageSize = 10;
  currentPage = 1;

  constructor(private userService: UserService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  get pagedUsers(): UserResponse[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredUsers.slice(start, start + this.pageSize);
  }

  avatarFor(name: string): string {
    return getInitials(name);
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
      u.displayName.toLowerCase().includes(q)
    );
    this.currentPage = 1;
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.cdr.detectChanges();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.cdr.detectChanges();
  }
}
