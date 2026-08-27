import { Component, computed, input, output, signal } from '@angular/core';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { Question, SubmitAnswer } from '../../core/test-api';

// Shared by the placement test and every reinforcement quiz - both are
// "answer a list of MCQ questions, track progress, submit once all are
// answered." What happens after submit differs per caller, so grading
// and navigation stay in the parent page, not here.
@Component({
  selector: 'app-quiz',
  imports: [MatRadioModule, MatButtonModule, MatProgressBarModule],
  templateUrl: './quiz.html',
  styleUrl: './quiz.scss',
})
export class Quiz {
  readonly questions = input.required<Question[]>();
  readonly submitting = input(false);
  readonly submitLabel = input('Submit');
  readonly submitted = output<SubmitAnswer[]>();

  private readonly selections = signal<Record<string, string>>({});

  protected readonly answeredCount = computed(() => Object.keys(this.selections()).length);
  protected readonly progress = computed(() =>
    this.questions().length === 0 ? 0 : (this.answeredCount() / this.questions().length) * 100,
  );
  protected readonly allAnswered = computed(
    () => this.questions().length > 0 && this.answeredCount() === this.questions().length,
  );

  select(questionId: string, optionId: string): void {
    this.selections.update((current) => ({ ...current, [questionId]: optionId }));
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
}
