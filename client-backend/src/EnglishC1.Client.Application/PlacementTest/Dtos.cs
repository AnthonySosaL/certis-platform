using EnglishC1.Client.Domain.PlacementTest;

namespace EnglishC1.Client.Application.PlacementTest;

// Never carries CorrectOptionId to the client - grading happens server-side only.
public record QuestionDto(Guid Id, string Text, SkillArea SkillArea, CefrLevel Level, List<QuestionOptionDto> Options);

public record QuestionOptionDto(Guid Id, string Text);

public record SubmitAnswerDto(Guid QuestionId, Guid SelectedOptionId);

public record SkillBreakdownDto(CefrLevel Level, SkillArea SkillArea, int Correct, int Total, bool NeedsReinforcement);

// Only built for questions the test-taker got wrong - shown after
// submission so a failed attempt teaches something instead of just
// reporting a score. Never sent before or during answering.
public record MissedQuestionDto(
    Guid QuestionId,
    string QuestionText,
    string YourAnswerText,
    string CorrectAnswerText,
    string? Explanation);

public record TestResultDto(
    Guid AttemptId,
    AttemptKind Kind,
    int Score,
    int TotalQuestions,
    CefrLevel? PlacementResult,
    DateTime CompletedAtUtc,
    List<SkillBreakdownDto> Breakdown,
    List<MissedQuestionDto> MissedQuestions);
