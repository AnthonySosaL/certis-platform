import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { Auth } from '../../core/auth';
import { AdminApi, Account, AdminQuestion, StudentSummary } from '../../core/admin-api';
import { levelCode, levelCssVar, levelName } from '../../core/cefr';
import { skillIconPath } from '../../core/skill-icons';
import { QuestionEditorDialog, QuestionEditorData } from './question-editor-dialog/question-editor-dialog';

type Tab = 'students' | 'content' | 'access';

@Component({
  selector: 'app-admin',
  imports: [MatProgressSpinnerModule, MatButtonModule, MatSlideToggleModule],
  templateUrl: './admin.html',
  styleUrl: './admin.scss',
})
export class AdminDashboard implements OnInit {
  private readonly adminApi = inject(AdminApi);
  private readonly dialog = inject(MatDialog);
  protected readonly auth = inject(Auth);

  protected readonly tab = signal<Tab>('students');

  // Students
  protected readonly studentsLoading = signal(true);
  protected readonly students = signal<StudentSummary[]>([]);
  protected readonly studentsError = signal<string | null>(null);
  protected readonly warningCount = computed(() => this.students().filter((s) => s.hasEarlyWarning).length);

  // Content
  protected readonly questionsLoading = signal(false);
  protected readonly questionsLoaded = signal(false);
  protected readonly questions = signal<AdminQuestion[]>([]);
  protected readonly questionsError = signal<string | null>(null);
  protected readonly questionGroups = computed(() => {
    const groups = new Map<string, AdminQuestion[]>();
    for (const q of this.questions()) {
      const key = `${q.level} ${q.skillArea}`;
      groups.set(key, [...(groups.get(key) ?? []), q]);
    }
    return [...groups.entries()].sort(([a], [b]) => a.localeCompare(b));
  });

  // Access
  protected readonly accountsLoading = signal(false);
  protected readonly accountsLoaded = signal(false);
  protected readonly accounts = signal<Account[]>([]);
  protected readonly accountsError = signal<string | null>(null);
  protected readonly accountsActionError = signal<string | null>(null);
  protected readonly savingAccountId = signal<string | null>(null);

  protected readonly levelCssVar = levelCssVar;
  protected readonly levelCode = levelCode;
  protected readonly levelName = levelName;
  protected readonly skillIconPath = skillIconPath;

  async ngOnInit(): Promise<void> {
    await this.loadStudents();
  }

  async selectTab(tab: Tab): Promise<void> {
    this.tab.set(tab);
    if (tab === 'content' && !this.questionsLoaded()) await this.loadQuestions();
    if (tab === 'access' && !this.accountsLoaded()) await this.loadAccounts();
  }

  private async loadStudents(): Promise<void> {
    try {
      this.students.set(await this.adminApi.getStudents());
    } catch {
      this.studentsError.set('Could not load student data.');
    } finally {
      this.studentsLoading.set(false);
    }
  }

  private async loadQuestions(): Promise<void> {
    this.questionsLoading.set(true);
    try {
      this.questions.set(await this.adminApi.getQuestions());
      this.questionsLoaded.set(true);
    } catch {
      this.questionsError.set('Could not load the question bank.');
    } finally {
      this.questionsLoading.set(false);
    }
  }

  private async loadAccounts(): Promise<void> {
    this.accountsLoading.set(true);
    try {
      this.accounts.set(await this.adminApi.getAccounts());
      this.accountsLoaded.set(true);
    } catch {
      this.accountsError.set('Could not load accounts.');
    } finally {
      this.accountsLoading.set(false);
    }
  }

  openQuestionEditor(question: AdminQuestion | null): void {
    const ref = this.dialog.open<QuestionEditorDialog, QuestionEditorData, AdminQuestion | undefined>(
      QuestionEditorDialog,
      { data: { question }, panelClass: 'content-dialog-panel', autoFocus: false },
    );
    ref.afterClosed().subscribe((saved) => {
      if (!saved) return;
      const current = this.questions();
      const index = current.findIndex((q) => q.id === saved.id);
      this.questions.set(index >= 0 ? current.map((q, i) => (i === index ? saved : q)) : [...current, saved]);
    });
  }

  async deleteQuestion(question: AdminQuestion): Promise<void> {
    if (!confirm(`Delete "${question.text}"? This can't be undone.`)) return;
    try {
      await this.adminApi.deleteQuestion(question.id);
      this.questions.set(this.questions().filter((q) => q.id !== question.id));
    } catch {
      this.questionsError.set('Could not delete this question.');
    }
  }

  async toggleRole(account: Account, role: 'isAdmin' | 'isTutor', checked: boolean): Promise<void> {
    this.savingAccountId.set(account.userId);
    this.accountsActionError.set(null);
    const request = { isAdmin: account.isAdmin, isTutor: account.isTutor, [role]: checked };
    try {
      const updated = await this.adminApi.setRoles(account.userId, request);
      this.accounts.set(this.accounts().map((a) => (a.userId === updated.userId ? updated : a)));
    } catch (error) {
      const message =
        error instanceof HttpErrorResponse && error.error?.message
          ? error.error.message
          : "Could not update that account's access.";
      this.accountsActionError.set(message);
    } finally {
      this.savingAccountId.set(null);
    }
  }

  formatDate(iso: string | null): string {
    if (!iso) return 'Never';
    return new Date(iso).toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' });
  }
}
