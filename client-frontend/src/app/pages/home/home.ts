import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

import { Auth } from '../../core/auth';
import { AuthDialogService } from '../../core/auth-dialog.service';

const STEPS = [
  {
    title: 'Take the placement test',
    text: '32 short questions covering grammar and vocabulary, from A2 to C1. About 15 minutes, no prep needed.',
  },
  {
    title: 'See exactly where you stand',
    text: 'A clear CEFR level, plus a breakdown of which specific areas are solid and which need work - no guessing.',
  },
  {
    title: 'Reinforce your weak spots',
    text: "Anything you scored under 60% on unlocks a short, focused quiz just for that topic. Retake it whenever you're ready.",
  },
];

const LEVELS: { level: string; cssVar: string; label: string }[] = [
  { level: 'A2', cssVar: '--cefr-a2', label: 'Elementary' },
  { level: 'B1', cssVar: '--cefr-b1', label: 'Intermediate' },
  { level: 'B2', cssVar: '--cefr-b2', label: 'Upper-Intermediate' },
  { level: 'C1', cssVar: '--cefr-c1', label: 'Advanced' },
];

@Component({
  imports: [MatButtonModule],
  selector: 'app-home',
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly authDialog = inject(AuthDialogService);

  protected readonly steps = STEPS;
  protected readonly levels = LEVELS;

  startTest(): void {
    if (this.auth.isAuthenticated()) {
      this.router.navigateByUrl('/test');
    } else {
      this.authDialog.open('register', '/test');
    }
  }
}
