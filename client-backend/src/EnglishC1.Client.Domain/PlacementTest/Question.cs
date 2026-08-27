namespace EnglishC1.Client.Domain.PlacementTest;

public class Question
{
    public Guid Id { get; init; }
    public required string Text { get; init; }
    public SkillArea SkillArea { get; init; }
    public CefrLevel Level { get; init; }
    public Guid CorrectOptionId { get; set; }
    public List<QuestionOption> Options { get; set; } = [];
}
