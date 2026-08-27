import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { Auth } from '../../core/auth';

function passwordsMatch(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return password === confirmPassword ? null : { passwordsMismatch: true };
}

// ASP.NET Core's ValidationProblem() shape: { errors: { Code: [message, ...] } }.
// Shows the real reason (e.g. "email already taken" vs. an actual password
// rule) instead of guessing - a previous version of this guessed wrong and
// sent someone looking for a duplicate account that didn't exist.
function extractIdentityErrors(error: unknown): string[] {
  if (!(error instanceof HttpErrorResponse)) return [];
  const errors = error.error?.errors as Record<string, string[]> | undefined;
  if (!errors) return [];
  return Object.values(errors).flat();
}

@Component({
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  selector: 'app-register',
  styleUrl: './register.scss',
  templateUrl: './register.html',
})
export class Register {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group(
    {
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatch },
  );

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    const { email, password } = this.form.getRawValue();

    try {
      await this.auth.register(email, password);
      await this.router.navigateByUrl('/');
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
