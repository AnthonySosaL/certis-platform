using EnglishC1.Client.Domain.PlacementTest;

namespace EnglishC1.Client.Application.PlacementTest;

// Never carries CorrectOptionId to the client - grading happens server-side only.
public record QuestionDto(Guid Id, string Text, SkillArea SkillArea, CefrLevel Level, List<QuestionOptionDto> Options);

public record QuestionOptionDto(Guid Id, string Text);

public record SubmitAnswerDto(Guid QuestionId, Guid SelectedOptionId);

public record SkillBreakdownDto(CefrLevel Level, SkillArea SkillArea, int Correct, int Total, bool NeedsReinforcement);

public record TestResultDto(
    Guid AttemptId,
    AttemptKind Kind,
    int Score,
    int TotalQuestions,
    CefrLevel? PlacementResult,
    DateTime CompletedAtUtc,
    List<SkillBreakdownDto> Breakdown);
