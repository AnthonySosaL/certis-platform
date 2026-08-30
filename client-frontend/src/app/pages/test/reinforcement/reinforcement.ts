import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { CefrLevel, Question, SkillArea, SubmitAnswer, TestApi, TestResult } from '../../../core/test-api';
import { passed as isPassed } from '../../../core/grading';
import { Quiz, readPersistedQuiz } from '../../../shared/quiz/quiz';

type Stage = 'intro' | 'lesson-loading' | 'lesson' | 'loading' | 'taking' | 'submitting' | 'result' | 'generating';

@Component({
  selector: 'app-reinforcement',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule, Quiz],
  templateUrl: './reinforcement.html',
  styleUrl: './reinforcement.scss',
})
export class Reinforcement implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly testApi = inject(TestApi);

  protected readonly level = this.route.snapshot.paramMap.get('level') as CefrLevel;
  protected readonly skill = this.route.snapshot.paramMap.get('skill') as SkillArea;
  protected readonly storageKey = `reinforcement-in-progress-${this.level}-${this.skill}`;

  protected readonly stage = signal<Stage>('intro');
  protected readonly questions = signal<Question[]>([]);
  protected readonly result = signal<TestResult | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly usingAiQuestions = signal(false);
  protected readonly lessonText = signal<string | null>(null);

  // A reload mid-quiz should resume straight into it, not re-show the
  // lesson-or-skip choice - that choice only makes sense once, at the
  // very start of a fresh attempt.
  ngOnInit(): void {
    const persisted = readPersistedQuiz(this.storageKey);
    if (persisted && persisted.hasAnswers) {
      this.questions.set(persisted.questions);
      this.stage.set('taking');
      return;
    }
    this.stage.set('intro');
  }

  // Optional pre-quiz mini-lesson (2026-08-29) - explicitly requested:
  // practicing here was "just more tests" with nowhere to actually
  // learn the material first. A real Groq call, generated fresh per
  // (level, skill) - never blocks the quiz itself if it fails.
  async takeLesson(): Promise<void> {
    this.stage.set('lesson-loading');
    this.errorMessage.set(null);
    try {
      const { content } = await this.testApi.getLesson(this.level, this.skill);
      this.lessonText.set(content);
      this.stage.set('lesson');
    } catch {
      this.errorMessage.set("The lesson isn't available right now.");
      this.stage.set('intro');
    }
  }

  async startQuiz(): Promise<void> {
    this.stage.set('loading');
    this.errorMessage.set(null);
    try {
      this.questions.set(await this.testApi.getReinforcementQuestions(this.level, this.skill));
      this.stage.set('taking');
    } catch {
      this.errorMessage.set('Could not load this quiz. Please try again.');
      this.stage.set('intro');
    }
  }

  async onSubmit(answers: SubmitAnswer[]): Promise<void> {
    this.stage.set('submitting');
    try {
      const result = await this.testApi.submitReinforcement(this.level, this.skill, answers);
      localStorage.removeItem(this.storageKey);
      this.result.set(result);
      this.stage.set('result');
    } catch {
      this.errorMessage.set('Could not submit your answers. Please try again.');
      this.stage.set('taking');
    }
  }

  passed(result: TestResult): boolean {
    return isPassed(result.score, result.totalQuestions);
  }

  // Re-practicing right after seeing a result skips straight back into
  // a quiz - the lesson-or-skip choice already happened once for this
  // attempt.
  async retry(): Promise<void> {
    this.result.set(null);
    this.usingAiQuestions.set(false);
    await this.startQuiz();
  }

  // Swaps the fixed bank for freshly Groq-generated questions on the
  // exact same (level, skill) - a different way to practice the same
  // weak area instead of seeing the identical few questions every time.
  async practiceWithAi(): Promise<void> {
    this.stage.set('generating');
    this.errorMessage.set(null);
    localStorage.removeItem(this.storageKey);
    try {
      const generated = await this.testApi.generateReinforcementQuestions(this.level, this.skill);
      this.questions.set(generated);
      this.usingAiQuestions.set(true);
      this.stage.set('taking');
    } catch {
      this.errorMessage.set("AI-generated practice isn't available right now.");
      this.stage.set('taking');
    }
  }
}
