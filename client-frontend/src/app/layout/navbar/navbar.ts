import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';

import { Theme } from '../../core/theme';
import { Auth } from '../../core/auth';

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

  protected readonly navItems = NAV_ITEMS;
  // Placeholder brand text until the final project name is picked - see docs/NAMING.md.
  protected readonly brandName = '[Project name]';

  signOut(): void {
    this.auth.logout();
    this.router.navigateByUrl('/');
  }
}
