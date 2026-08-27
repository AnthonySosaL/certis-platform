import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { Auth } from '../../core/auth';

export type AuthMode = 'login' | 'register';

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

// Sign in and register used to be two separate routed pages. Merged into
// one component with an internal mode toggle (no navigation between the
// two - the router isn't involved in switching) so it reads as a single
// modal-style card that slides between panels, per the request. `/login`
// and `/register` both still route here, just with a different initial
// `mode` (route data) - deep links and the auth guard's `redirectTo`
// keep working unchanged.
@Component({
  imports: [ReactiveFormsModule, RouterLink, MatFormFieldModule, MatInputModule, MatButtonModule, MatProgressSpinnerModule],
  selector: 'app-auth-page',
  styleUrl: './auth-page.scss',
  templateUrl: './auth-page.html',
})
export class AuthPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly mode = signal<AuthMode>((this.route.snapshot.data['mode'] as AuthMode) ?? 'login');
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

  async onLoginSubmit(): Promise<void> {
    if (this.loginForm.invalid || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    const { email, password } = this.loginForm.getRawValue();

    try {
      await this.auth.login(email, password);
      await this.redirectAfterAuth();
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
      await this.redirectAfterAuth();
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

  private async redirectAfterAuth(): Promise<void> {
    const redirectTo = this.route.snapshot.queryParamMap.get('redirectTo');
    await this.router.navigateByUrl(redirectTo ?? '/');
  }
}
