import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { TestApi, TestResult } from '../../core/test-api';
import { Auth } from '../../core/auth';
import { levelCode, levelCssVar, levelName } from '../../core/cefr';
import { skillIconPath } from '../../core/skill-icons';

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

  async ngOnInit(): Promise<void> {
    this.history.set(await this.testApi.getHistory());
    this.loading.set(false);
  }

  passed(result: TestResult): boolean {
    return result.totalQuestions > 0 && result.score / result.totalQuestions >= 0.6;
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' });
  }
}
