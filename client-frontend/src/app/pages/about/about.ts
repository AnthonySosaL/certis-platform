import { AfterViewInit, Component, ElementRef, QueryList, ViewChildren } from '@angular/core';
import { RouterLink } from '@angular/router';

interface Section {
  title: string;
  text: string;
  iconPath: string;
}

const SECTIONS: Section[] = [
  {
    title: 'Format',
    text: "The placement test is a fixed set of 64 multiple-choice questions answered in one sitting - a mix of questions for each combination of level (A2, B1, B2, C1) and skill (Grammar, Vocabulary, Reading, Listening). It isn't adaptive: everyone sees the same questions, which keeps grading transparent and repeatable.",
    // Document/checklist
    iconPath: 'M9 12h6M9 16h6M9 8h2M6 4h9l3 3v13a1 1 0 0 1-1 1H6a1 1 0 0 1-1-1V5a1 1 0 0 1 1-1Z',
  },
  {
    title: 'How the level is calculated',
    text: 'Each (level, skill) group is graded out of 10, school-style, and counts as "passed" at 7/10 or higher. Your placement is the highest level where every group from A2 up to that level was passed, consecutively. Scoring well on C1 vocabulary doesn\'t place you at C1 if there are real gaps in B1 grammar - CEFR placement is meant to reflect a consistent level, not a single strong area.',
    // Ascending bars / scale
    iconPath: 'M4 20V12M10 20V4M16 20V9M22 20V14',
  },
  {
    title: 'Reinforcement',
    text: 'Any (level, skill) group graded under 7/10 - even above your overall placement - is flagged for reinforcement. Each flagged area unlocks a short, focused quiz covering just that topic, so practice time goes toward specific, identified gaps instead of a generic review.',
    // Target
    iconPath: 'M12 2v4M12 18v4M2 12h4M18 12h4M12 8a4 4 0 1 0 0 8 4 4 0 0 0 0-8Z',
  },
  {
    title: 'Scope, honestly',
    text: 'This version covers Grammar, Vocabulary, Reading, and Listening - Writing and Speaking aren\'t part of the scored placement test yet, though Speaking now has its own AI roleplay practice section. The question bank is mostly hand-written, with AI-generated questions available for extra reinforcement practice, and is small enough that results should be read as a solid estimate, not a certified exam score.',
    // Eye
    iconPath: 'M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7Z M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z',
  },
];

@Component({
  imports: [RouterLink],
  selector: 'app-about',
  styleUrl: './about.scss',
  templateUrl: './about.html',
})
export class About implements AfterViewInit {
  protected readonly sections = SECTIONS;

  @ViewChildren('revealItem') private revealItems!: QueryList<ElementRef<HTMLElement>>;

  ngAfterViewInit(): void {
    if (typeof IntersectionObserver === 'undefined') {
      // No-op fallback (e.g. very old browsers, SSR) - just show everything.
      this.revealItems.forEach((item) => item.nativeElement.classList.add('is-visible'));
      return;
    }

    const observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            entry.target.classList.add('is-visible');
            observer.unobserve(entry.target);
          }
        }
      },
      { threshold: 0.15, rootMargin: '0px 0px -40px 0px' },
    );

    this.revealItems.forEach((item) => observer.observe(item.nativeElement));
  }
}
