import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

// Honest placeholder, not a fake flow: real password reset needs an
// email sender wired up server-side (SendGrid free tier or similar -
// see docs/PENDING_IDEAS.md), which isn't configured yet. Rather than
// ship a form that silently does nothing, this says so plainly.
@Component({
  imports: [RouterLink, MatButtonModule],
  selector: 'app-forgot-password',
  styleUrl: './forgot-password.scss',
  templateUrl: './forgot-password.html',
})
export class ForgotPassword {}
