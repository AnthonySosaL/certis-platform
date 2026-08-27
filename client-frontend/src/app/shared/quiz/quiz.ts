import { Component, DestroyRef, OnInit, computed, inject, input, output, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { Question, SubmitAnswer } from '../../core/test-api';

const OPTION_LETTERS = ['A', 'B', 'C', 'D', 'E', 'F'];

interface PersistedState {
  selections: Record<string, string>;
  startedAtMs: number;
  // The exact questions shown for this attempt - needed to resume
  // straight into the quiz on reload instead of re-fetching. Re-fetching
  // isn't safe to do silently: placement questions are now sampled per
  // call (see TestService.GetPlacementQuestionsAsync), so a fresh fetch
  // can return a different subset whose IDs don't match the persisted
  // selections, silently orphaning progress the user thinks was saved.
  questions: Question[];
}

// Lets a parent page check for (and restore) an in-progress attempt
// before Quiz itself ever mounts - e.g. to skip a "ready to start?" intro
// screen and land the user straight back in the quiz after a reload.
export function readPersistedQuiz(storageKey: string): { questions: Question[]; hasAnswers: boolean } | null {
  if (typeof localStorage === 'undefined') return null;

  const raw = localStorage.getItem(storageKey);
  if (!raw) return null;

  try {
    const state = JSON.parse(raw) as Partial<PersistedState>;
    if (!state.questions || state.questions.length === 0) return null;
    return { questions: state.questions, hasAnswers: Object.keys(state.selections ?? {}).length > 0 };
  } catch {
    return null;
  }
}

// Shared by the placement test and every reinforcement quiz - both are
// "answer a list of MCQ questions, track progress, submit once all are
// answered." What happens after submit differs per caller, so grading
// and navigation stay in the parent page, not here.
//
// Deliberately not a native mat-radio-group: the "selected option" look
// here (letter badge, fill, underline, check) is custom enough that
// re-skinning Material's MDC radio internals fought the design more than
// it helped - plain clickable rows with role="radio" cover the same
// keyboard/screen-reader contract without that friction.
//
// When `storageKey` is set, answers and the elapsed timer survive a
// reload or a lost connection - restored from localStorage on init,
// re-persisted on every change, and cleared only once the parent
// confirms a submit actually succeeded (call `clearPersisted()`).
@Component({
  selector: 'app-quiz',
  imports: [MatButtonModule, MatProgressBarModule],
  templateUrl: './quiz.html',
  styleUrl: './quiz.scss',
})
export class Quiz implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  readonly questions = input.required<Question[]>();
  readonly submitting = input(false);
  readonly submitLabel = input('Submit');
  readonly storageKey = input<string | null>(null);
  readonly submitted = output<SubmitAnswer[]>();

  private readonly selections = signal<Record<string, string>>({});
  private readonly startedAtMs = signal(Date.now());
  protected readonly elapsedSeconds = signal(0);

  protected readonly answeredCount = computed(() => Object.keys(this.selections()).length);
  protected readonly progress = computed(() =>
    this.questions().length === 0 ? 0 : (this.answeredCount() / this.questions().length) * 100,
  );
  protected readonly allAnswered = computed(
    () => this.questions().length > 0 && this.answeredCount() === this.questions().length,
  );
  protected readonly elapsedLabel = computed(() => {
    const total = this.elapsedSeconds();
    const minutes = Math.floor(total / 60);
    const seconds = total % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  });

  ngOnInit(): void {
    this.restore();

    const timer = setInterval(() => {
      this.elapsedSeconds.set(Math.floor((Date.now() - this.startedAtMs()) / 1000));
    }, 1000);
    this.destroyRef.onDestroy(() => clearInterval(timer));
  }

  letterFor(index: number): string {
    return OPTION_LETTERS[index] ?? String(index + 1);
  }

  select(questionId: string, optionId: string): void {
    this.selections.update((current) => ({ ...current, [questionId]: optionId }));
    this.persist();
  }

  selected(questionId: string): string | undefined {
    return this.selections()[questionId];
  }

  submit(): void {
    if (!this.allAnswered()) return;
    const answers: SubmitAnswer[] = Object.entries(this.selections()).map(([questionId, selectedOptionId]) => ({
      questionId,
      selectedOptionId,
    }));
    this.submitted.emit(answers);
  }

  // Call after a submit is confirmed to have actually succeeded - a
  // failed submit deliberately keeps the draft so the retry doesn't
  // lose answers.
  clearPersisted(): void {
    const key = this.storageKey();
    if (key && typeof localStorage !== 'undefined') localStorage.removeItem(key);
  }

  private restore(): void {
    const key = this.storageKey();
    if (!key || typeof localStorage === 'undefined') return;

    const raw = localStorage.getItem(key);
    if (!raw) {
      // Lock in the real start time now, so a reload before the first
      // answer still resumes the timer from when the attempt actually began.
      this.persist();
      return;
    }

    try {
      const state = JSON.parse(raw) as PersistedState;
      this.selections.set(state.selections ?? {});
      this.startedAtMs.set(state.startedAtMs ?? Date.now());
      this.elapsedSeconds.set(Math.floor((Date.now() - this.startedAtMs()) / 1000));
    } catch {
      localStorage.removeItem(key);
    }
  }

  private persist(): void {
    const key = this.storageKey();
    if (!key || typeof localStorage === 'undefined') return;

    const state: PersistedState = {
      selections: this.selections(),
      startedAtMs: this.startedAtMs(),
      questions: this.questions(),
    };
    localStorage.setItem(key, JSON.stringify(state));
  }
}
