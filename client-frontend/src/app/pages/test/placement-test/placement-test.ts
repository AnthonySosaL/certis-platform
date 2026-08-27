import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { Question, SubmitAnswer, TestApi, TestResult } from '../../../core/test-api';
import { Quiz, readPersistedQuiz } from '../../../shared/quiz/quiz';

type Stage = 'loading' | 'intro' | 'taking' | 'submitting';

// Answers + elapsed time persist under this key (see shared/quiz/quiz.ts)
// so losing the connection mid-test and coming back doesn't lose
// progress - the quiz was getting destroyed and recreated on every
// stage change even before that, so this also fixes resuming after a
// failed submit.
const STORAGE_KEY = 'placement-test-in-progress';

@Component({
  selector: 'app-placement-test',
  imports: [MatButtonModule, MatProgressSpinnerModule, RouterLink, Quiz],
  templateUrl: './placement-test.html',
  styleUrl: './placement-test.scss',
})
export class PlacementTest implements OnInit {
  private readonly testApi = inject(TestApi);
  private readonly router = inject(Router);

  protected readonly storageKey = STORAGE_KEY;
  protected readonly stage = signal<Stage>('loading');
  protected readonly previousResult = signal<TestResult | null>(null);
  protected readonly questions = signal<Question[]>([]);
  protected readonly errorMessage = signal<string | null>(null);

  async ngOnInit(): Promise<void> {
    try {
      this.previousResult.set(await this.testApi.getLatestPlacementResult());
    } catch {
      // Best-effort - if this fails, we just skip straight to "no previous result" state.
    }

    // A reload (or a lost connection) mid-test used to always land back on
    // this intro screen, requiring one click on "Retake the test" before
    // the preserved answers/timer became visible again. Restoring the
    // exact question set here instead skips that click entirely - it also
    // has to be the *exact* set, not a fresh fetch, since placement
    // questions are sampled per call (see readPersistedQuiz's comment).
    const persisted = readPersistedQuiz(STORAGE_KEY);
    if (persisted && persisted.hasAnswers) {
      this.questions.set(persisted.questions);
      this.stage.set('taking');
      return;
    }

    this.stage.set('intro');
  }

  async start(): Promise<void> {
    this.stage.set('loading');
    try {
      this.questions.set(await this.testApi.getPlacementQuestions());
      this.stage.set('taking');
    } catch {
      this.errorMessage.set('Could not load the test questions. Please try again.');
      this.stage.set('intro');
    }
  }

  async onSubmit(answers: SubmitAnswer[]): Promise<void> {
    this.stage.set('submitting');
    try {
      await this.testApi.submitPlacementTest(answers);
      localStorage.removeItem(STORAGE_KEY);
      await this.router.navigateByUrl('/test/results');
    } catch {
      this.errorMessage.set('Could not submit your answers. Please try again.');
      this.stage.set('taking');
    }
  }
}
