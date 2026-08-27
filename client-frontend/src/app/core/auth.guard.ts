import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';

import { Auth } from './auth';
import { AuthDialogService } from './auth-dialog.service';

// Blocks navigation (no route, nothing to redirect to) and opens the
// sign-in dialog instead - the guard runs before the app leaves whatever
// page the user was already on, so returning false just keeps them
// there with the modal now open over it. On success the dialog itself
// navigates to `state.url`.
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(Auth);
  const authDialog = inject(AuthDialogService);

  if (auth.isAuthenticated()) return true;

  authDialog.open('login', state.url);
  return false;
};
