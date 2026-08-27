namespace EnglishC1.Client.Domain.PlacementTest;

public enum AttemptKind
{
    // The full 32-question test across all levels/skills - determines the CEFR placement.
    Placement,

    // A short quiz scoped to one (Level, SkillArea) cell the user scored
    // below the pass threshold on. Doesn't change the stored placement -
    // see TestAttempt.FocusLevel/FocusSkill.
    Reinforcement,
}
