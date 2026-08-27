import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';

import { Theme } from '../../core/theme';
import { Auth } from '../../core/auth';
import { AuthDialogService } from '../../core/auth-dialog.service';

type NavItem = { path: string; label: string };

const BASE_NAV_ITEMS: NavItem[] = [
  { path: '/', label: 'Home' },
  { path: '/test', label: 'Take the test' },
  { path: '/about', label: 'About' },
];

const DASHBOARD_ITEM: NavItem = { path: '/dashboard', label: 'Dashboard' };
const ADMIN_ITEM: NavItem = { path: '/admin', label: 'Admin' };

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

  protected readonly navItems = computed<NavItem[]>(() => {
    if (!this.auth.isAuthenticated()) return BASE_NAV_ITEMS;
    return this.auth.canManage()
      ? [...BASE_NAV_ITEMS, DASHBOARD_ITEM, ADMIN_ITEM]
      : [...BASE_NAV_ITEMS, DASHBOARD_ITEM];
  });

  openSignIn(): void {
    this.authDialog.open('login');
  }

  signOut(): void {
    this.auth.logout();
    this.router.navigateByUrl('/');
  }
}
