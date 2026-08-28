import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { CefrLevel, Question, SkillArea, SubmitAnswer, TestApi, TestResult } from '../../../core/test-api';
import { passed as isPassed } from '../../../core/grading';
import { Quiz } from '../../../shared/quiz/quiz';

type Stage = 'loading' | 'taking' | 'submitting' | 'result' | 'generating';

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

  protected readonly stage = signal<Stage>('loading');
  protected readonly questions = signal<Question[]>([]);
  protected readonly result = signal<TestResult | null>(null);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly usingAiQuestions = signal(false);

  async ngOnInit(): Promise<void> {
    try {
      this.questions.set(await this.testApi.getReinforcementQuestions(this.level, this.skill));
      this.stage.set('taking');
    } catch {
      this.errorMessage.set('Could not load this quiz. Please try again.');
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

  async retry(): Promise<void> {
    this.result.set(null);
    this.usingAiQuestions.set(false);
    this.stage.set('loading');
    await this.ngOnInit();
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
