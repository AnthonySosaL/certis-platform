import { Component } from '@angular/core';

// Static, no backend - a continuously scrolling strip of short tips
// shown below the navbar on every page. The template renders this list
// twice back to back so the CSS animation (translateX 0 -> -50%) loops
// seamlessly with no visible jump or gap.
const TIPS: string[] = [
  'Anything graded under 7/10 unlocks a focused reinforcement quiz for that topic.',
  'Practice any skill anytime from Courses - no placement test required.',
  'Grammar tip: "since" marks a starting point, "for" marks a duration.',
  'Try Speaking practice - a text roleplay with an AI Cambridge examiner.',
  'Vocabulary tip: "meticulous" means paying close attention to detail.',
  'Listening tip: read the question before playing the audio.',
  'Reading tip: skim for the main idea before answering the question.',
  'Your placement level only updates when you retake the full test.',
];

@Component({
  selector: 'app-tips-ticker',
  templateUrl: './tips-ticker.html',
  styleUrl: './tips-ticker.scss',
})
export class TipsTicker {
  protected readonly tips = TIPS;
}
