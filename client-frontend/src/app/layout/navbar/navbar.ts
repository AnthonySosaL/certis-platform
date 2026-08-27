import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';

import { Theme } from '../../core/theme';
import { Auth } from '../../core/auth';
import { AuthDialogService } from '../../core/auth-dialog.service';

type NavItem = { path: string; label: string };

const NAV_ITEMS: NavItem[] = [
  { path: '/', label: 'Home' },
  { path: '/test', label: 'Take the test' },
  { path: '/about', label: 'About' },
];

@Component({
  imports: [RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatMenuModule],
  selector: 'app-navbar',
  styleUrl: './navbar.scss',
  templateUrl: './navbar.html',
})
export class Navbar {
  protected readonly theme = inject(Theme);
  protected readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly authDialog = inject(AuthDialogService);

  protected readonly navItems = NAV_ITEMS;

  openSignIn(): void {
    this.authDialog.open('login');
  }

  signOut(): void {
    this.auth.logout();
    this.router.navigateByUrl('/');
  }
}
