import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { CefrLevel, CourseSlide, SkillArea, TestApi } from '../../core/test-api';

type Stage = 'loading' | 'ready' | 'unavailable';

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

// The real multi-slide course (2026-08-29), distinct from the shorter
// pre-quiz "lesson" on the reinforcement page - explicitly requested:
// "de ley debe haber un curso con algo de texto, imagenes, ideas...
// segun sea grammar... y en su nivel respectivo". Reached from the
// Courses hub, before practicing.
@Component({
  selector: 'app-course',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './course.html',
  styleUrl: './course.scss',
})
export class Course implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly testApi = inject(TestApi);

  protected readonly level = this.route.snapshot.paramMap.get('level') as CefrLevel;
  protected readonly skill = this.route.snapshot.paramMap.get('skill') as SkillArea;
  protected readonly headerImage = SKILL_IMAGE[this.skill] ?? SKILL_IMAGE['Grammar'];

  protected readonly stage = signal<Stage>('loading');
  protected readonly slides = signal<CourseSlide[]>([]);
  protected readonly index = signal(0);

  protected readonly currentSlide = computed(() => this.slides()[this.index()] ?? null);
  protected readonly isFirst = computed(() => this.index() === 0);
  protected readonly isLast = computed(() => this.index() === this.slides().length - 1);

  async ngOnInit(): Promise<void> {
    try {
      const { slides } = await this.testApi.getCourse(this.level, this.skill);
      this.slides.set(slides);
      this.stage.set('ready');
    } catch {
      this.stage.set('unavailable');
    }
  }

  next(): void {
    if (!this.isLast()) this.index.update((i) => i + 1);
  }

  previous(): void {
    if (!this.isFirst()) this.index.update((i) => i - 1);
  }
}
