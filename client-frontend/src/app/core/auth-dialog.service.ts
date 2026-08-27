import { Service, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';

import { AuthDialog, AuthDialogData, AuthDialogResult } from '../pages/auth/auth-dialog';

export type AuthMode = 'login' | 'register';

// Opens sign-in/register as a real overlay (MatDialog) - no route
// change, so it can appear over whatever page the user was already on
// (the navbar, the home CTA, or the auth guard blocking a protected
// route). Replaces the earlier version that routed to /login and
// /register - that only *looked* like a modal.
@Service()
export class AuthDialogService {
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);

  open(mode: AuthMode, redirectTo?: string): void {
    if (this.dialog.openDialogs.length > 0) return;

    const ref = this.dialog.open<AuthDialog, AuthDialogData, AuthDialogResult>(AuthDialog, {
      data: { mode, redirectTo },
      panelClass: 'auth-dialog-panel',
      autoFocus: false,
    });

    ref.afterClosed().subscribe((result) => {
      if (result?.redirectTo) {
        this.router.navigateByUrl(result.redirectTo);
      }
    });
  }
}
