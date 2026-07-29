import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { UserResponse } from '../../models/user-response';
import { UpdateUserRequest } from '../../models/update-user-request';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.html',
  standalone: false
})
export class Profile implements OnInit {
  user: UserResponse | null = null;
  editModel: UpdateUserRequest = {};
  editing = false;
  loading = false;
  saving = false;
  message = '';
  error = '';

  constructor(private auth: AuthService, private userService: UserService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    const userId = this.auth.getCurrentUserId();
    if (userId) {
      this.loading = true;
      this.cdr.detectChanges();
      this.userService.getById(userId).subscribe({
        next: u => {
          this.user = u;
          this.editModel = { userName: u.userName, email: u.email, displayName: u.displayName };
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.error = 'Failed to load profile.';
          this.loading = false;
          this.cdr.detectChanges();
        }
      });
    }
  }

  startEdit(): void {
    this.editing = true;
    this.message = '';
    this.error = '';
  }

  cancelEdit(): void {
    this.editing = false;
    if (this.user) {
      this.editModel = { userName: this.user.userName, email: this.user.email, displayName: this.user.displayName };
    }
  }

  save(): void {
    if (!this.user) return;
    this.saving = true;
    this.message = '';
    this.error = '';
    this.cdr.detectChanges();
    this.userService.update(this.user.id, this.editModel).subscribe({
      next: updated => {
        this.saving = false;
        this.user = updated;
        this.editing = false;
        this.message = 'Profile updated successfully.';
        this.cdr.detectChanges();
      },
      error: err => {
        this.saving = false;
        this.error = err.error?.error ?? 'Update failed.';
        this.cdr.detectChanges();
      }
    });
  }
}
