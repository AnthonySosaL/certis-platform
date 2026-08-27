namespace EnglishC1.Client.Domain.PlacementTest;

public class TestAttempt
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public AttemptKind Kind { get; init; }

    // Only set for Kind == Reinforcement - which (level, skill) cell this quiz targeted.
    public CefrLevel? FocusLevel { get; init; }
    public SkillArea? FocusSkill { get; init; }

    public DateTime StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }

    // Only meaningful for Kind == Placement - the highest level passed consecutively from A2.
    public CefrLevel? PlacementResult { get; set; }

    public List<TestAnswer> Answers { get; init; } = [];
}
