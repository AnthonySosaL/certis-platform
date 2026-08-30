import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { CefrLevel, CourseSlide, Question, SkillArea, SubmitAnswer, TestApi, TestResult } from '../../core/test-api';
import { passed as isPassed } from '../../core/grading';
import { Quiz } from '../../shared/quiz/quiz';

type Stage =
  | 'choice'
  | 'course-loading'
  | 'course-ready'
  | 'course-unavailable'
  | 'mini-loading'
  | 'mini-taking'
  | 'mini-submitting'
  | 'mini-result';

// One header image per skill, reused across every level's course for
// that skill - matches the pattern already used for Home/About's
// photo sections (a hand-picked Pexels photo, downloaded once, no live
// API call at runtime).
const SKILL_IMAGE: Record<string, { src: string; credit: string }> = {
  Grammar: { src: '/images/home-practice.jpg', credit: 'Photo by Tima Miroshnichenko / Pexels' },
  Vocabulary: { src: '/images/about-hero.jpg', credit: 'Photo by Polina Tankilevitch / Pexels' },
  Reading: { src: '/images/course-reading.jpg', credit: 'Photo by cottonbro studio / Pexels' },
  Listening: { src: '/images/home-cta.jpg', credit: 'Photo by Anna Shvets / Pexels' },
};

const MINI_QUIZ_COUNT = 3;

// Reached from the Courses hub, before practicing (2026-08-29/30) -
// explicitly requested: "de ley debe haber un curso con algo de texto,
// imagenes, ideas". Opens on a real choice between three paths (added
// 2026-08-30 after direct feedback that a single forced path felt
// disconnected from the general test): take the full multi-slide
// course, take a short AI-generated mini-quiz to gauge readiness, or
// skip straight to the real (graded) quiz.
@Component({
  selector: 'app-course',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule, Quiz],
  templateUrl: './course.html',
  styleUrl: './course.scss',
})
export class Course {
  private readonly route = inject(ActivatedRoute);
  private readonly testApi = inject(TestApi);

  protected readonly level = this.route.snapshot.paramMap.get('level') as CefrLevel;
  protected readonly skill = this.route.snapshot.paramMap.get('skill') as SkillArea;
  protected readonly headerImage = SKILL_IMAGE[this.skill] ?? SKILL_IMAGE['Grammar'];

  protected readonly stage = signal<Stage>('choice');
  protected readonly errorMessage = signal<string | null>(null);

  // Full course (slide deck)
  protected readonly slides = signal<CourseSlide[]>([]);
  protected readonly index = signal(0);
  protected readonly currentSlide = computed(() => this.slides()[this.index()] ?? null);
  protected readonly isFirst = computed(() => this.index() === 0);
  protected readonly isLast = computed(() => this.index() === this.slides().length - 1);

  // Ungraded self-check state for the current drag/write slide - reset
  // whenever the slide changes.
  protected readonly exerciseResult = signal<'correct' | 'incorrect' | null>(null);
  protected readonly droppedAnswer = signal<string | null>(null);
  protected readonly writeInput = signal('');

  // Mini-quiz (a short round of AI-generated questions through the same
  // real reinforcement pipeline as "Practice different questions" -
  // graded and recorded like any other attempt, just shorter).
  protected readonly miniQuestions = signal<Question[]>([]);
  protected readonly miniResult = signal<TestResult | null>(null);
  protected readonly miniPassed = computed(() => {
    const r = this.miniResult();
    return r ? isPassed(r.score, r.totalQuestions) : false;
  });

  async takeCourse(): Promise<void> {
    this.stage.set('course-loading');
    this.errorMessage.set(null);
    try {
      const { slides } = await this.testApi.getCourse(this.level, this.skill);
      this.slides.set(slides);
      this.index.set(0);
      this.resetExercise();
      this.stage.set('course-ready');
    } catch {
      this.stage.set('course-unavailable');
    }
  }

  async takeMiniQuiz(): Promise<void> {
    this.stage.set('mini-loading');
    this.errorMessage.set(null);
    try {
      this.miniQuestions.set(await this.testApi.generateReinforcementQuestions(this.level, this.skill, MINI_QUIZ_COUNT));
      this.stage.set('mini-taking');
    } catch {
      this.errorMessage.set("The quick check isn't available right now.");
      this.stage.set('choice');
    }
  }

  async onMiniSubmit(answers: SubmitAnswer[]): Promise<void> {
    this.stage.set('mini-submitting');
    try {
      const result = await this.testApi.submitReinforcement(this.level, this.skill, answers);
      this.miniResult.set(result);
      this.stage.set('mini-result');
    } catch {
      this.errorMessage.set('Could not grade the quick check. Please try again.');
      this.stage.set('mini-taking');
    }
  }

  backToChoice(): void {
    this.stage.set('choice');
    this.errorMessage.set(null);
  }

  next(): void {
    if (!this.isLast()) {
      this.index.update((i) => i + 1);
      this.resetExercise();
    }
  }

  previous(): void {
    if (!this.isFirst()) {
      this.index.update((i) => i - 1);
      this.resetExercise();
    }
  }

  private resetExercise(): void {
    this.exerciseResult.set(null);
    this.droppedAnswer.set(null);
    this.writeInput.set('');
  }

  private checkAnswer(given: string): void {
    const slide = this.currentSlide();
    if (!slide?.answer) return;
    const isCorrect = given.trim().toLowerCase() === slide.answer.trim().toLowerCase();
    this.exerciseResult.set(isCorrect ? 'correct' : 'incorrect');
  }

  onDragStart(event: DragEvent, option: string): void {
    event.dataTransfer?.setData('text/plain', option);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    const option = event.dataTransfer?.getData('text/plain');
    if (!option) return;
    this.droppedAnswer.set(option);
    this.checkAnswer(option);
  }

  // Tap-to-place fallback for touch devices, where native HTML5
  // drag-and-drop doesn't work without extra plumbing - same outcome as
  // dropping the chip.
  pickOption(option: string): void {
    this.droppedAnswer.set(option);
    this.checkAnswer(option);
  }

  setWriteInput(value: string): void {
    this.writeInput.set(value);
  }

  checkWrite(): void {
    this.checkAnswer(this.writeInput());
  }
}
