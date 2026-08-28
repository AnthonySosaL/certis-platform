const SKILL_ICON_PATH: Record<string, string> = {
  // Open book - Grammar (structure/rules)
  Grammar: 'M4 19.5A2.5 2.5 0 0 1 6.5 17H20 M4 19.5A2.5 2.5 0 0 0 6.5 22H20V4H6.5A2.5 2.5 0 0 0 4 6.5v13Z',
  // Speech bubble - Vocabulary (words/expression)
  Vocabulary: 'M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z',
  // Document with text lines - Reading (a passage to read)
  Reading: 'M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z M14 2v6h6 M8 13h8 M8 17h8 M8 9h2',
  // Headphones - Listening (a clip to listen to)
  Listening: 'M3 14v-2a9 9 0 0 1 18 0v2 M21 14v4a2 2 0 0 1-2 2h-1v-8h1a2 2 0 0 1 2 2z M3 14v4a2 2 0 0 0 2 2h1v-8H5a2 2 0 0 0-2 2z',
};

export function skillIconPath(skill: string): string {
  return SKILL_ICON_PATH[skill] ?? SKILL_ICON_PATH['Grammar'];
}
