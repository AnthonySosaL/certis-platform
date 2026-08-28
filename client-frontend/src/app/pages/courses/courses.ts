import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CefrLevel, SkillArea } from '../../core/test-api';
import { skillIconPath } from '../../core/skill-icons';

const LEVELS: CefrLevel[] = ['A2', 'B1', 'B2', 'C1'];

interface SkillCourse {
  skill: SkillArea;
  title: string;
  cambridgePart: string;
  description: string;
}

// "Ver cursos" (2026-08-28) - a free-practice hub, not gated behind a
// failed placement-test area first. GetReinforcementQuestionsAsync never
// required a prior attempt (see TestController.cs) - this page is just
// the first real front door into that already-open endpoint, organized
// the same way a Cambridge exam is structured: Grammar/Vocabulary
// (Use of English), Reading, Listening, and Speaking.
const SKILL_COURSES: SkillCourse[] = [
  {
    skill: 'Grammar',
    title: 'Grammar',
    cambridgePart: 'Use of English',
    description: 'Tenses, conditionals, modals, and the structures that trip people up at every level.',
  },
  {
    skill: 'Vocabulary',
    title: 'Vocabulary',
    cambridgePart: 'Use of English',
    description: 'Word choice, phrasal verbs, and meaning-in-context questions.',
  },
  {
    skill: 'Reading',
    title: 'Reading',
    cambridgePart: 'Reading',
    description: 'Short passages with a comprehension question, growing in length and register by level.',
  },
  {
    skill: 'Listening',
    title: 'Listening',
    cambridgePart: 'Listening',
    description: 'Short spoken clips - announcements, conversations, opinions - with a comprehension question.',
  },
];

@Component({
  selector: 'app-courses',
  imports: [RouterLink],
  templateUrl: './courses.html',
  styleUrl: './courses.scss',
})
export class Courses {
  protected readonly levels = LEVELS;
  protected readonly skillCourses = SKILL_COURSES;
  protected readonly skillIconPath = skillIconPath;
}
