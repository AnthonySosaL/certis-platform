const SKILL_ICON_PATH: Record<string, string> = {
  // Open book - Grammar (structure/rules)
  Grammar: 'M4 19.5A2.5 2.5 0 0 1 6.5 17H20 M4 19.5A2.5 2.5 0 0 0 6.5 22H20V4H6.5A2.5 2.5 0 0 0 4 6.5v13Z',
  // Speech bubble - Vocabulary (words/expression)
  Vocabulary: 'M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z',
};

export function skillIconPath(skill: string): string {
  return SKILL_ICON_PATH[skill] ?? SKILL_ICON_PATH['Grammar'];
}
