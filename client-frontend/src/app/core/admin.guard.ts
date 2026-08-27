import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { Auth } from './auth';
import { AuthDialogService } from './auth-dialog.service';

// Same not-signed-in handling as authGuard (open the dialog in place),
// plus a hard redirect home for a signed-in account with neither role -
// there's no "apply for access" flow, so there's nothing useful to show
// them on this route. Admin and Tutor both get in; the page itself hides
// the Admin-only Access tab for a Tutor.
export const adminGuard: CanActivateFn = (_route, state) => {
  const auth = inject(Auth);
  const authDialog = inject(AuthDialogService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    authDialog.open('login', state.url);
    return false;
  }

  if (!auth.canManage()) {
    router.navigateByUrl('/');
    return false;
  }

  return true;
};
