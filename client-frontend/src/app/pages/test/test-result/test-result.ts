import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { CefrLevel, TestApi, TestResult } from '../../../core/test-api';
import { levelCode, levelCssVar, levelName } from '../../../core/cefr';
import { skillIconPath } from '../../../core/skill-icons';

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

  protected readonly levelCssVar = levelCssVar;
  protected readonly levelCode = levelCode;
  protected readonly levelName = levelName;
  protected readonly skillIconPath = skillIconPath;

  async ngOnInit(): Promise<void> {
    this.result.set(await this.testApi.getLatestPlacementResult());
    this.loading.set(false);
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
