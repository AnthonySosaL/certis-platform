import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';

import { Theme } from '../../core/theme';
import { Auth } from '../../core/auth';
import { AuthDialogService } from '../../core/auth-dialog.service';

type NavItem = { path: string; label: string };

// "About" stays last regardless of auth state - explicitly requested
// (2026-08-29) so the practice-related links (Take the test, Dashboard,
// Courses, Speaking) group together first, with About as the closing
// "learn more" item rather than sitting in the middle of them.
const BASE_NAV_ITEMS: NavItem[] = [
  { path: '/', label: 'Home' },
  { path: '/test', label: 'Take the test' },
];
const ABOUT_ITEM: NavItem = { path: '/about', label: 'About' };

const DASHBOARD_ITEM: NavItem = { path: '/dashboard', label: 'Dashboard' };
const COURSES_ITEM: NavItem = { path: '/courses', label: 'Courses' };
const SPEAKING_ITEM: NavItem = { path: '/speaking', label: 'Speaking' };
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
    if (!this.auth.isAuthenticated()) return [...BASE_NAV_ITEMS, ABOUT_ITEM];
    return this.auth.canManage()
      ? [...BASE_NAV_ITEMS, DASHBOARD_ITEM, COURSES_ITEM, SPEAKING_ITEM, ADMIN_ITEM, ABOUT_ITEM]
      : [...BASE_NAV_ITEMS, DASHBOARD_ITEM, COURSES_ITEM, SPEAKING_ITEM, ABOUT_ITEM];
  });

  openSignIn(): void {
    this.authDialog.open('login');
  }

  signOut(): void {
    this.auth.logout();
    this.router.navigateByUrl('/');
  }
}
