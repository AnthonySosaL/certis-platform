import { Component, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AdminApi, AdminQuestion, UpsertQuestionRequest } from '../../../core/admin-api';
import { CefrLevel, SkillArea } from '../../../core/test-api';

export interface QuestionEditorData {
  question: AdminQuestion | null;
}

const LEVELS: CefrLevel[] = ['A2', 'B1', 'B2', 'C1'];
const SKILLS: SkillArea[] = ['Grammar', 'Vocabulary', 'Reading'];

@Component({
  selector: 'app-question-editor-dialog',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatRadioModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './question-editor-dialog.html',
  styleUrl: './question-editor-dialog.scss',
})
export class QuestionEditorDialog {
  private readonly fb = inject(FormBuilder);
  private readonly adminApi = inject(AdminApi);
  private readonly ref = inject(MatDialogRef<QuestionEditorDialog, AdminQuestion | undefined>);
  private readonly data = inject<QuestionEditorData>(MAT_DIALOG_DATA);

  protected readonly levels = LEVELS;
  protected readonly skills = SKILLS;
  protected readonly isEditing = this.data.question !== null;
  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    text: [this.data.question?.text ?? '', Validators.required],
    level: [this.data.question?.level ?? 'A2', Validators.required],
    skillArea: [this.data.question?.skillArea ?? 'Grammar', Validators.required],
    explanation: [this.data.question?.explanation ?? ''],
    passage: [this.data.question?.passage ?? ''],
    options: this.fb.array(
      (this.data.question?.options ?? [{ text: '' }, { text: '' }]).map((o) =>
        this.fb.nonNullable.control(o.text, Validators.required),
      ),
    ),
    correctOptionIndex: [this.initialCorrectIndex(), Validators.required],
  });

  protected readonly isReading = signal(this.form.controls.skillArea.value === 'Reading');

  constructor() {
    this.form.controls.skillArea.valueChanges.subscribe((skill) => this.isReading.set(skill === 'Reading'));
  }

  private initialCorrectIndex(): number {
    if (!this.data.question) return 0;
    const index = this.data.question.options.findIndex((o) => o.id === this.data.question!.correctOptionId);
    return index >= 0 ? index : 0;
  }

  get options(): FormArray {
    return this.form.controls.options as FormArray;
  }

  addOption(): void {
    this.options.push(this.fb.nonNullable.control('', Validators.required));
  }

  removeOption(index: number): void {
    if (this.options.length <= 2) return;
    this.options.removeAt(index);
    const correct = this.form.controls.correctOptionIndex.value;
    if (correct === index) this.form.controls.correctOptionIndex.setValue(0);
    else if (correct > index) this.form.controls.correctOptionIndex.setValue(correct - 1);
  }

  cancel(): void {
    this.ref.close(undefined);
  }

  async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    if (value.skillArea === 'Reading' && value.passage.trim() === '') {
      this.errorMessage.set('Reading questions need a passage.');
      return;
    }

    const request: UpsertQuestionRequest = {
      text: value.text,
      level: value.level,
      skillArea: value.skillArea,
      explanation: value.explanation.trim() === '' ? null : value.explanation,
      passage: value.passage.trim() === '' ? null : value.passage,
      options: value.options,
      correctOptionIndex: value.correctOptionIndex,
    };

    this.saving.set(true);
    this.errorMessage.set(null);
    try {
      const saved = this.data.question
        ? await this.adminApi.updateQuestion(this.data.question.id, request)
        : await this.adminApi.createQuestion(request);
      this.ref.close(saved);
    } catch {
      this.errorMessage.set('Could not save this question. Please try again.');
      this.saving.set(false);
    }
  }
}
