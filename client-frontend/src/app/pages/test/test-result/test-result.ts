import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { CefrLevel, TestApi, TestResult } from '../../../core/test-api';

const LEVEL_CSS_VAR: Record<CefrLevel, string> = {
  A2: '--cefr-a2',
  B1: '--cefr-b1',
  B2: '--cefr-b2',
  C1: '--cefr-c1',
};

@Component({
  selector: 'app-test-result',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './test-result.html',
  styleUrl: './test-result.scss',
})
export class TestResultPage implements OnInit {
  private readonly testApi = inject(TestApi);

  protected readonly loading = signal(true);
  protected readonly result = signal<TestResult | null>(null);

  async ngOnInit(): Promise<void> {
    this.result.set(await this.testApi.getLatestPlacementResult());
    this.loading.set(false);
  }

  levelCssVar(level: CefrLevel): string {
    return `var(${LEVEL_CSS_VAR[level]})`;
  }

  reinforcementPath(level: CefrLevel, skill: string): string[] {
    return ['/test/reinforce', level, skill];
  }
}
