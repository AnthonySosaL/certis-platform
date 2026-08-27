namespace EnglishC1.Client.Domain.PlacementTest;

public class QuestionOption
{
    public Guid Id { get; init; }
    public Guid QuestionId { get; init; }
    public required string Text { get; init; }
}
