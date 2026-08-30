import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { TestApi, TestResult } from '../../core/test-api';
import { Auth } from '../../core/auth';
import { levelCode, levelCssVar, levelName } from '../../core/cefr';
import { skillIconPath } from '../../core/skill-icons';
import { passed as isPassed } from '../../core/grading';

type InsightState = 'idle' | 'loading' | 'ready' | 'unavailable' | 'error';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private readonly testApi = inject(TestApi);
  protected readonly auth = inject(Auth);

  protected readonly loading = signal(true);
  protected readonly history = signal<TestResult[]>([]);

  protected readonly placementHistory = computed(() =>
    this.history().filter((r) => r.kind === 'Placement'),
  );
  protected readonly reinforcementHistory = computed(() =>
    this.history().filter((r) => r.kind === 'Reinforcement'),
  );
  protected readonly latestPlacement = computed(() => this.placementHistory()[0] ?? null);

  protected readonly levelCssVar = levelCssVar;
  protected readonly levelCode = levelCode;
  protected readonly levelName = levelName;
  protected readonly skillIconPath = skillIconPath;

  // "Coach" panel - a real Groq call across the student's FULL history
  // (2026-08-29), not tied to any single attempt. Was previously buried
  // as a per-attempt button on the placement results page, which the
  // user pointed out was barely useful there - moved here, more
  // visible, and scoped to overall progress instead of one attempt.
  protected readonly insightState = signal<InsightState>('idle');
  protected readonly insightText = signal<string | null>(null);

  async ngOnInit(): Promise<void> {
    this.history.set(await this.testApi.getHistory());
    this.loading.set(false);
  }

  passed(result: TestResult): boolean {
    return isPassed(result.score, result.totalQuestions);
  }

  async getOverallInsight(): Promise<void> {
    this.insightState.set('loading');
    try {
      const { insight } = await this.testApi.getOverallInsight();
      this.insightText.set(insight);
      this.insightState.set('ready');
    } catch (error) {
      const status = error instanceof HttpErrorResponse ? error.status : null;
      this.insightState.set(status === 503 ? 'unavailable' : 'error');
    }
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' });
  }
}
