namespace EnglishC1.Client.Domain.PlacementTest;

public class Question
{
    public Guid Id { get; init; }
    public required string Text { get; init; }
    public SkillArea SkillArea { get; init; }
    public CefrLevel Level { get; init; }
    public Guid CorrectOptionId { get; set; }
    public List<QuestionOption> Options { get; set; } = [];

    // Shown only after a wrong answer is submitted (never during the
    // test itself) - a short reason so a failed reinforcement attempt
    // teaches something instead of just reporting a score.
    public string? Explanation { get; set; }
}
