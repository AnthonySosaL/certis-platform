import { CUSTOM_ELEMENTS_SCHEMA, Component, OnInit, computed, inject, signal } from '@angular/core';
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

interface ModelOption {
  src: string;
  label: string;
  alt: string;
}

// Candidates only - swap this array down to one entry (or restyle
// entirely) once a final pick is made. All three are CC0 (public
// domain) low-poly models from Poly Pizza, downloaded to
// public/models/ - see docs/STRUCTURE_CHANGELOG.md for the direct
// source URLs.
const MODEL_OPTIONS: ModelOption[] = [
  { src: '/models/open-book.glb', label: 'Open Book', alt: 'A low-poly open book' },
  { src: '/models/grad-cap.glb', label: 'Graduation Cap', alt: 'A low-poly graduation cap' },
  { src: '/models/globe.glb', label: 'Globe', alt: 'A low-poly globe' },
];

@Component({
  imports: [MatButtonModule],
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

  protected readonly models = MODEL_OPTIONS;
  protected readonly activeModelIndex = signal(0);
  protected readonly activeModel = computed(() => this.models[this.activeModelIndex()]);

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

  previousModel(): void {
    this.activeModelIndex.update((i) => (i - 1 + this.models.length) % this.models.length);
  }

  nextModel(): void {
    this.activeModelIndex.update((i) => (i + 1) % this.models.length);
  }

  selectModel(index: number): void {
    this.activeModelIndex.set(index);
  }
}
