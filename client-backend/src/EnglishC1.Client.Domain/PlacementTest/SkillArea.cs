namespace EnglishC1.Client.Domain.PlacementTest;

public enum SkillArea
{
    Grammar,
    Vocabulary,

    // 2026-08-28: first new skill beyond the original two. Carries a
    // Question.Passage (a short paragraph) that Grammar/Vocabulary
    // questions don't use - see Question.cs.
    Reading,

    // 2026-08-28: second new skill from the same request. Carries a
    // Question.AudioUrl (a short spoken clip) instead of a Passage - see
    // Question.cs.
    Listening,
}
