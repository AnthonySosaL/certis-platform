namespace EnglishC1.Client.Domain.PlacementTest;

public class TestAnswer
{
    public Guid Id { get; init; }
    public Guid TestAttemptId { get; set; }
    public Guid QuestionId { get; init; }
    public Guid SelectedOptionId { get; init; }
    public bool IsCorrect { get; init; }
}
