import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';

import { Auth } from '../../core/auth';

export type AuthMode = 'login' | 'register';
export interface AuthDialogData {
  mode: AuthMode;
  redirectTo?: string;
}
export interface AuthDialogResult {
  redirectTo?: string;
}

function passwordsMatch(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return password === confirmPassword ? null : { passwordsMismatch: true };
}

// ASP.NET Core's ValidationProblem() shape: { errors: { Code: [message, ...] } }.
// Shows the real reason (e.g. "email already taken" vs. an actual password
// rule) instead of guessing.
function extractIdentityErrors(error: unknown): string[] {
  if (!(error instanceof HttpErrorResponse)) return [];
  const errors = error.error?.errors as Record<string, string[]> | undefined;
  if (!errors) return [];
  return Object.values(errors).flat();
}

// Opened via AuthDialogService (MatDialog), never routed to directly -
// it's the real overlay behind "Sign in" / a blocked protected route,
// not a page. Sign in and Register are two forms in one instance with
// an internal `mode` toggle (a local signal, not navigation) so a CSS
// slide animates between them.
@Component({
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatProgressSpinnerModule],
  selector: 'app-auth-dialog',
  styleUrl: './auth-dialog.scss',
  templateUrl: './auth-dialog.html',
})
export class AuthDialog {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(Auth);
  private readonly dialogRef = inject(MatDialogRef<AuthDialog, AuthDialogResult>);
  private readonly data = inject<AuthDialogData>(MAT_DIALOG_DATA);
  private readonly router = inject(Router);

  protected readonly mode = signal<AuthMode>(this.data.mode);
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  // Must stay in sync with the backend's Identity.Password options in
  // client-backend/.../DependencyInjection.cs.
  protected readonly passwordHint = 'At least 8 characters, including a number.';

  protected readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  protected readonly registerForm = this.fb.nonNullable.group(
    {
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/\d/)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatch },
  );

  switchTo(mode: AuthMode): void {
    if (this.mode() === mode) return;
    this.errorMessage.set(null);
    this.mode.set(mode);
  }

  close(): void {
    this.dialogRef.close();
  }

  goToForgotPassword(): void {
    this.dialogRef.close();
    this.router.navigateByUrl('/forgot-password');
  }

  async onLoginSubmit(): Promise<void> {
    if (this.loginForm.invalid || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    const { email, password } = this.loginForm.getRawValue();

    try {
      await this.auth.login(email, password);
      this.dialogRef.close({ redirectTo: this.data.redirectTo ?? '/' });
    } catch {
      this.errorMessage.set('Invalid email or password.');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  async onRegisterSubmit(): Promise<void> {
    if (this.registerForm.invalid || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    const { email, password } = this.registerForm.getRawValue();

    try {
      await this.auth.register(email, password);
      this.dialogRef.close({ redirectTo: this.data.redirectTo ?? '/' });
    } catch (error) {
      const identityErrors = extractIdentityErrors(error);
      this.errorMessage.set(
        identityErrors.length > 0
          ? identityErrors.join(' ')
          : 'Could not create the account. Please try again.',
      );
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
