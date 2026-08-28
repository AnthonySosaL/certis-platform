import { Service, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { API_BASE_URL } from './api-config';
import { CefrLevel, SkillArea } from './test-api';

export interface WeakArea {
  level: CefrLevel;
  skillArea: SkillArea;
}

export interface StudentSummary {
  userId: string;
  email: string;
  latestPlacementLevel: CefrLevel | null;
  latestPlacementDate: string | null;
  totalAttempts: number;
  weakAreas: WeakArea[];
  hasEarlyWarning: boolean;
}

export interface AdminQuestionOption {
  id: string;
  text: string;
}

export interface AdminQuestion {
  id: string;
  text: string;
  level: CefrLevel;
  skillArea: SkillArea;
  explanation: string | null;
  options: AdminQuestionOption[];
  correctOptionId: string;
  isAiGenerated: boolean;
  passage: string | null;
}

export interface UpsertQuestionRequest {
  text: string;
  level: CefrLevel;
  skillArea: SkillArea;
  explanation: string | null;
  options: string[];
  correctOptionIndex: number;
  passage: string | null;
}

export interface Account {
  userId: string;
  email: string;
  isAdmin: boolean;
  isTutor: boolean;
  tutorId: string | null;
  tutorLabel: string | null;
}

export interface SetRolesRequest {
  isAdmin: boolean;
  isTutor: boolean;
}

export interface SetTutorRequest {
  tutorUserId: string | null;
}

// Thin wrapper over /api/admin/* - mirrors TestApi's shape. Student and
// question endpoints require Admin or Tutor; account/role endpoints
// require Admin only (401/403 server-side otherwise either way).
@Service()
export class AdminApi {
  private readonly http = inject(HttpClient);

  getStudents(): Promise<StudentSummary[]> {
    return firstValueFrom(this.http.get<StudentSummary[]>(`${API_BASE_URL}/api/admin/students`));
  }

  getQuestions(): Promise<AdminQuestion[]> {
    return firstValueFrom(this.http.get<AdminQuestion[]>(`${API_BASE_URL}/api/admin/questions`));
  }

  createQuestion(request: UpsertQuestionRequest): Promise<AdminQuestion> {
    return firstValueFrom(this.http.post<AdminQuestion>(`${API_BASE_URL}/api/admin/questions`, request));
  }

  updateQuestion(id: string, request: UpsertQuestionRequest): Promise<AdminQuestion> {
    return firstValueFrom(this.http.put<AdminQuestion>(`${API_BASE_URL}/api/admin/questions/${id}`, request));
  }

  deleteQuestion(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API_BASE_URL}/api/admin/questions/${id}`));
  }

  getAccounts(): Promise<Account[]> {
    return firstValueFrom(this.http.get<Account[]>(`${API_BASE_URL}/api/admin/accounts`));
  }

  setRoles(userId: string, request: SetRolesRequest): Promise<Account> {
    return firstValueFrom(this.http.put<Account>(`${API_BASE_URL}/api/admin/accounts/${userId}/roles`, request));
  }

  setTutor(userId: string, request: SetTutorRequest): Promise<Account> {
    return firstValueFrom(this.http.put<Account>(`${API_BASE_URL}/api/admin/accounts/${userId}/tutor`, request));
  }
}
