import { Service, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { API_BASE_URL } from './api-config';

export type CefrLevel = 'A2' | 'B1' | 'B2' | 'C1';
export type SkillArea = 'Grammar' | 'Vocabulary';
export type AttemptKind = 'Placement' | 'Reinforcement';

export interface QuestionOption {
  id: string;
  text: string;
}

export interface Question {
  id: string;
  text: string;
  skillArea: SkillArea;
  level: CefrLevel;
  options: QuestionOption[];
}

export interface SubmitAnswer {
  questionId: string;
  selectedOptionId: string;
}

export interface SkillBreakdown {
  level: CefrLevel;
  skillArea: SkillArea;
  correct: number;
  total: number;
  needsReinforcement: boolean;
  grade: number;
}

export interface MissedQuestion {
  questionId: string;
  questionText: string;
  yourAnswerText: string;
  correctAnswerText: string;
  explanation: string | null;
}

export interface TestInsight {
  insight: string;
}

export interface TestResult {
  attemptId: string;
  kind: AttemptKind;
  score: number;
  totalQuestions: number;
  grade: number;
  placementResult: CefrLevel | null;
  completedAtUtc: string;
  breakdown: SkillBreakdown[];
  missedQuestions: MissedQuestion[];
}

// Thin wrapper over /api/test/* - mirrors the shape of core/auth.ts
// (plain HttpClient calls via firstValueFrom, no state caching here since
// each page fetches what it needs directly).
@Service()
export class TestApi {
  private readonly http = inject(HttpClient);

  getPlacementQuestions(): Promise<Question[]> {
    return firstValueFrom(this.http.get<Question[]>(`${API_BASE_URL}/api/test/placement/questions`));
  }

  submitPlacementTest(answers: SubmitAnswer[]): Promise<TestResult> {
    return firstValueFrom(this.http.post<TestResult>(`${API_BASE_URL}/api/test/placement/submit`, answers));
  }

  getLatestPlacementResult(): Promise<TestResult | null> {
    return firstValueFrom(this.http.get<TestResult>(`${API_BASE_URL}/api/test/results/placement/latest`)).catch(
      (error) => {
        if (error?.status === 404) return null;
        throw error;
      },
    );
  }

  getHistory(): Promise<TestResult[]> {
    return firstValueFrom(this.http.get<TestResult[]>(`${API_BASE_URL}/api/test/results/history`));
  }

  // On-demand AI feedback for one attempt - a real network call (Groq),
  // never fired automatically. 503 from the backend means it isn't
  // configured/available right now, not a bug - callers should show that
  // distinctly from a generic error.
  getInsight(attemptId: string): Promise<TestInsight> {
    return firstValueFrom(
      this.http.post<TestInsight>(`${API_BASE_URL}/api/test/results/${attemptId}/insight`, {}),
    );
  }

  getReinforcementQuestions(level: CefrLevel, skill: SkillArea): Promise<Question[]> {
    return firstValueFrom(
      this.http.get<Question[]>(`${API_BASE_URL}/api/test/reinforcement/${level}/${skill}/questions`),
    );
  }

  submitReinforcement(level: CefrLevel, skill: SkillArea, answers: SubmitAnswer[]): Promise<TestResult> {
    return firstValueFrom(
      this.http.post<TestResult>(`${API_BASE_URL}/api/test/reinforcement/${level}/${skill}/submit`, answers),
    );
  }
}
