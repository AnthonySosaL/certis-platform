import { Service, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { API_BASE_URL } from './api-config';

export interface SpeakingScenario {
  id: string;
  title: string;
  level: string;
  description: string;
}

export interface SpeakingTurn {
  role: 'user' | 'assistant';
  content: string;
}

// Thin wrapper over /api/speaking/* - mirrors the shape of core/test-api.ts.
@Service()
export class SpeakingApi {
  private readonly http = inject(HttpClient);

  getScenarios(): Promise<SpeakingScenario[]> {
    return firstValueFrom(this.http.get<SpeakingScenario[]>(`${API_BASE_URL}/api/speaking/scenarios`));
  }

  // A real Groq call per turn, never automatic. 503 from the backend
  // means it isn't configured/available right now, not a bug - callers
  // should show that distinctly from a generic error.
  reply(scenarioId: string, history: SpeakingTurn[], message: string): Promise<string> {
    return firstValueFrom(
      this.http.post<{ reply: string }>(`${API_BASE_URL}/api/speaking/${scenarioId}/reply`, { history, message }),
    ).then((r) => r.reply);
  }
}
