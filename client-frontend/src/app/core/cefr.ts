import { CefrLevel } from './test-api';

// Shared level -> color/name mapping, used anywhere a CEFR level needs a
// consistent chip color or display name (results, breakdown cards, the
// dashboard). Pulled out once this started being copy-pasted a third
// time.
const LEVEL_CSS_VAR: Record<CefrLevel, string> = {
  A2: '--cefr-a2',
  B1: '--cefr-b1',
  B2: '--cefr-b2',
  C1: '--cefr-c1',
};

const LEVEL_NAME: Record<CefrLevel, string> = {
  A2: 'Elementary',
  B1: 'Intermediate',
  B2: 'Upper-Intermediate',
  C1: 'Advanced',
};

export function levelCssVar(level: CefrLevel): string {
  return `var(${LEVEL_CSS_VAR[level]})`;
}

export function levelCode(level: CefrLevel | null): string {
  return level ?? 'Pre-A2';
}

export function levelName(level: CefrLevel | null): string {
  return level ? LEVEL_NAME[level] : 'Beginner';
}
