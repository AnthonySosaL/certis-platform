using EnglishC1.Client.Domain.PlacementTest;

namespace EnglishC1.Client.Application.Ai;

// A freshly-generated question, not yet persisted - the caller decides
// whether/how to save it (TestService inserts it as a real Question so
// grading works through the exact same path as the hand-written bank).
public record GeneratedQuestion(string Text, List<string> Options, int CorrectIndex, string Explanation);

public interface IAiQuestionGeneratorService
{
    // Returns null if generation or validation failed (including after
    // an internal retry) - the caller should treat that as "couldn't
    // generate one this time", not a hard error. `avoidTexts` is the
    // existing bank for that (level, skill) cell, passed through so the
    // prompt can steer away from regenerating a near-duplicate.
    Task<GeneratedQuestion?> GenerateQuestionAsync(
        CefrLevel level,
        SkillArea skill,
        IReadOnlyCollection<string> avoidTexts,
        CancellationToken ct = default);
}
