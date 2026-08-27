namespace EnglishC1.Client.Domain.PlacementTest;

// Pure scoring/placement logic, deliberately kept free of EF Core or any
// infrastructure dependency so it's cheaply unit-testable - see
// EnglishC1.Client.Domain.Tests/PlacementTest/PlacementScorerTests.cs.
// See docs/errors or STRUCTURE_CHANGELOG.md (2026-08-27) for the
// reasoning behind the "consecutive from A2" placement rule and the 60%
// pass threshold.
public static class PlacementScorer
{
    public const double PassThreshold = 0.6;
    public static readonly CefrLevel[] LevelOrder = [CefrLevel.A2, CefrLevel.B1, CefrLevel.B2, CefrLevel.C1];

    public static List<SkillBreakdown> BuildBreakdown(IEnumerable<TestAnswer> answers, IReadOnlyDictionary<Guid, Question> questionsById)
    {
        return answers
            .Where(a => questionsById.ContainsKey(a.QuestionId))
            .GroupBy(a => (questionsById[a.QuestionId].Level, questionsById[a.QuestionId].SkillArea))
            .Select(group =>
            {
                var correct = group.Count(a => a.IsCorrect);
                var total = group.Count();
                return new SkillBreakdown(group.Key.Level, group.Key.SkillArea, correct, total, NeedsReinforcement: total > 0 && (double)correct / total < PassThreshold);
            })
            .OrderBy(b => Array.IndexOf(LevelOrder, b.Level))
            .ThenBy(b => b.SkillArea)
            .ToList();
    }

    // The highest level where every (level, skill) cell from A2 up to
    // that level passed - one weak cell caps the result there, even if
    // higher levels individually scored well. Returns null ("below A2")
    // if A2 itself wasn't fully passed.
    public static CefrLevel? ComputePlacement(IReadOnlyList<SkillBreakdown> breakdown)
    {
        CefrLevel? placement = null;

        foreach (var level in LevelOrder)
        {
            var cellsAtLevel = breakdown.Where(b => b.Level == level).ToList();
            if (cellsAtLevel.Count == 0) break;

            var passed = cellsAtLevel.All(c => !c.NeedsReinforcement);
            if (!passed) break;

            placement = level;
        }

        return placement;
    }
}
