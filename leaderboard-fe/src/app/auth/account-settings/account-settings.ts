import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService } from '../../core/auth-service';

@Component({
  selector: 'app-account-settings',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './account-settings.html',
  styleUrl: './account-settings.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountSettings {
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  private router = inject(Router);

  // Password change state
  passwordSuccess = signal<string | null>(null);
  passwordError = signal<string | null>(null);
  passwordSubmitting = signal(false);

  // Username change state
  usernameSuccess = signal<string | null>(null);
  usernameError = signal<string | null>(null);
  usernameSubmitting = signal(false);

  passwordForm = this.fb.group({
    oldPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: this.passwordsMatchValidator.bind(this) });

  usernameForm = this.fb.group({
    newUsername: ['', [Validators.required, Validators.minLength(3)]]
  });

  private passwordsMatchValidator(group: AbstractControl): ValidationErrors | null {
    const newPassword = group.get('newPassword')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return newPassword === confirm ? null : { passwordMismatch: true };
  }


  changePassword() {
    if (this.passwordForm.invalid) {
      return;
    }

    const { oldPassword, newPassword, confirmPassword } = this.passwordForm.value;

    if (newPassword !== confirmPassword || this.passwordForm.hasError('passwordMismatch')) {
      this.passwordError.set('New passwords do not match');
      return;
    }

    this.passwordSubmitting.set(true);
    this.passwordSuccess.set(null);
    this.passwordError.set(null);

    this.authService.changePassword(oldPassword!, newPassword!).subscribe({
      next: (success) => {
        this.passwordSubmitting.set(false);
        if (success) {
          this.passwordSuccess.set('Password changed successfully!');
          this.passwordForm.reset();
        } else {
          this.passwordError.set('Failed to change password. Please check your old password.');
        }
      },
      error: () => {
        this.passwordSubmitting.set(false);
        this.passwordError.set('An error occurred. Please try again.');
      }
    });
  }

  updateUsername() {
    if (this.usernameForm.invalid) {
      return;
    }

    const { newUsername } = this.usernameForm.value;

    this.usernameSubmitting.set(true);
    this.usernameSuccess.set(null);
    this.usernameError.set(null);

    this.authService.updateUsername(newUsername!).subscribe({
      next: (success) => {
        this.usernameSubmitting.set(false);
        if (success) {
          this.usernameSuccess.set('Username updated successfully! Please log in again.');
          this.usernameForm.reset();
          // Log out and redirect to login after username change
          setTimeout(() => {
            this.authService.logout();
            this.router.navigate(['/auth/login']);
          }, 2000);
        } else {
          this.usernameError.set('Failed to update username. It may already be taken.');
        }
      },
      error: () => {
        this.usernameSubmitting.set(false);
        this.usernameError.set('An error occurred. Please try again.');
      }
    });
  }

  // clear mismatch error when the user edits password fields
  private initPasswordChangeListener() {
    this.passwordForm.valueChanges.subscribe(() => {
      if (!this.passwordForm.hasError('passwordMismatch')) {
        this.passwordError.set(null);
      }
    });
  }

  constructor() {
    // initialize form listeners
    this.initPasswordChangeListener();
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
