import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

import { AuthDialogService } from '../../core/auth-dialog.service';

// Honest placeholder, not a fake flow: real password reset needs an
// email sender wired up server-side (SendGrid free tier or similar -
// see docs/PENDING_IDEAS.md), which isn't configured yet. Rather than
// ship a form that silently does nothing, this says so plainly.
@Component({
  imports: [MatButtonModule],
  selector: 'app-forgot-password',
  styleUrl: './forgot-password.scss',
  templateUrl: './forgot-password.html',
})
export class ForgotPassword {
  private readonly authDialog = inject(AuthDialogService);

  openSignIn(): void {
    this.authDialog.open('login');
  }
}
