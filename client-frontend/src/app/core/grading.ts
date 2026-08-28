// Mirrors PlacementScorer.PassThreshold (backend) - a school-style
// "below 7/10 needs attention" bar, explicitly requested (2026-08-28,
// raised from 60%). Third place this exact 0.6 was hardcoded
// (dashboard.ts, reinforcement.ts) before being pulled out here.
export const PASS_THRESHOLD = 0.7;

export function passed(score: number, total: number): boolean {
  return total > 0 && score / total >= PASS_THRESHOLD;
}
