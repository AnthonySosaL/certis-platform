import { Service, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

// Lightweight confirmations only ("Question added", "Access updated") -
// modals stay reserved for anything more consequential (submitting a
// graded exercise, deleting an account). Wraps MatSnackBar so call sites
// don't need to know panelClass/duration conventions - see styles.scss
// for the `.app-toast*` visual treatment.
@Service()
export class Toast {
  private readonly snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.snackBar.open(message, undefined, {
      duration: 3000,
      panelClass: ['app-toast', 'app-toast--success'],
      horizontalPosition: 'end',
      verticalPosition: 'bottom',
    });
  }

  error(message: string): void {
    this.snackBar.open(message, undefined, {
      duration: 4000,
      panelClass: ['app-toast', 'app-toast--error'],
      horizontalPosition: 'end',
      verticalPosition: 'bottom',
    });
  }
}
