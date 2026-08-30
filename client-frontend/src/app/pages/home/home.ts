import { CUSTOM_ELEMENTS_SCHEMA, Component, OnInit, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

import { Auth } from '../../core/auth';
import { AuthDialogService } from '../../core/auth-dialog.service';

const STEPS = [
  {
    title: 'Take the placement test',
    text: '64 short questions covering grammar, vocabulary, reading, and listening, from A2 to C1. About 25 minutes, no prep needed.',
  },
  {
    title: 'See exactly where you stand',
    text: 'A clear CEFR level, plus a breakdown of which specific areas are solid and which need work - no guessing.',
  },
  {
    title: 'Reinforce your weak spots',
    text: "Anything graded under 7/10 unlocks a short, focused quiz just for that topic. Retake it whenever you're ready.",
  },
];

const LEVELS: { level: string; cssVar: string; label: string }[] = [
  { level: 'A2', cssVar: '--cefr-a2', label: 'Elementary' },
  { level: 'B1', cssVar: '--cefr-b1', label: 'Intermediate' },
  { level: 'B2', cssVar: '--cefr-b2', label: 'Upper-Intermediate' },
  { level: 'C1', cssVar: '--cefr-c1', label: 'Advanced' },
];

// Picked from three CC0 candidates previewed live on this page - see
// docs/STRUCTURE_CHANGELOG.md (2026-08-27) for how they were sourced.
// The other two (grad-cap.glb, globe.glb) are kept in public/models/
// on purpose, reserved for another spot or a loading screen later -
// not dead files.
const HERO_MODEL = { src: '/models/open-book.glb', alt: 'A low-poly open book' };

@Component({
  imports: [MatButtonModule, RouterLink],
  selector: 'app-home',
  styleUrl: './home.scss',
  templateUrl: './home.html',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class Home implements OnInit {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly authDialog = inject(AuthDialogService);

  protected readonly steps = STEPS;
  protected readonly levels = LEVELS;
  protected readonly heroModel = HERO_MODEL;

  ngOnInit(): void {
    // Dynamic import, not a static one: @google/model-viewer is ~900KB
    // (it bundles its own three.js-based renderer) - eagerly importing
    // it at the top of this file put that weight in the MAIN bundle,
    // loaded on every page. This way it's a separate chunk, fetched
    // only when Home actually renders.
    import('@google/model-viewer');
  }

  startTest(): void {
    if (this.auth.isAuthenticated()) {
      this.router.navigateByUrl('/test');
    } else {
      this.authDialog.open('register', '/test');
    }
  }
}
