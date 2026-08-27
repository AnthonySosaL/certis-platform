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

const LEVEL_NAME: Record<CefrLevel, string> = {
  A2: 'Elementary',
  B1: 'Intermediate',
  B2: 'Upper-Intermediate',
  C1: 'Advanced',
};

const SKILL_ICON_PATH: Record<string, string> = {
  // Open book - Grammar (structure/rules)
  Grammar: 'M4 19.5A2.5 2.5 0 0 1 6.5 17H20 M4 19.5A2.5 2.5 0 0 0 6.5 22H20V4H6.5A2.5 2.5 0 0 0 4 6.5v13Z',
  // Speech bubble - Vocabulary (words/expression)
  Vocabulary: 'M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z',
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

  levelName(level: CefrLevel | null): string {
    return level ? LEVEL_NAME[level] : 'Just starting out';
  }

  skillIconPath(skill: string): string {
    return SKILL_ICON_PATH[skill] ?? SKILL_ICON_PATH['Grammar'];
  }

  reinforcementPath(level: CefrLevel, skill: string): string[] {
    return ['/test/reinforce', level, skill];
  }

  // SVG ring: circumference of r=82 is ~515.2; dasharray/offset trace the score's share of it.
  ringOffset(score: number, total: number): number {
    const circumference = 515.2;
    const share = total > 0 ? score / total : 0;
    return circumference * (1 - share);
  }
}
