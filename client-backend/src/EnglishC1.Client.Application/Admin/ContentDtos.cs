using EnglishC1.Client.Domain.PlacementTest;

namespace EnglishC1.Client.Application.Admin;

// Admin/tutor-facing question shape - unlike QuestionDto (PlacementTest),
// this exposes CorrectOptionId on purpose: it's only ever sent to
// Admin/Tutor-authorized routes, never to a test-taker.
public record AdminQuestionDto(
    Guid Id,
    string Text,
    CefrLevel Level,
    SkillArea SkillArea,
    string? Explanation,
    List<AdminQuestionOptionDto> Options,
    Guid CorrectOptionId,
    bool IsAiGenerated,
    string? Passage,
    string? AudioUrl);

public record AdminQuestionOptionDto(Guid Id, string Text);

public record UpsertQuestionRequest(
    string Text,
    CefrLevel Level,
    SkillArea SkillArea,
    string? Explanation,
    List<string> Options,
    int CorrectOptionIndex,
    string? Passage,
    string? AudioUrl);
