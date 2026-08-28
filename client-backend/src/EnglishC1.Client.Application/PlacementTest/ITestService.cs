using EnglishC1.Client.Domain.PlacementTest;

namespace EnglishC1.Client.Application.PlacementTest;

public interface ITestService
{
    Task<List<QuestionDto>> GetPlacementQuestionsAsync();
    Task<TestResultDto> SubmitPlacementTestAsync(Guid userId, List<SubmitAnswerDto> answers);
    Task<TestResultDto?> GetLatestResultAsync(Guid userId, AttemptKind kind);
    Task<List<TestResultDto>> GetHistoryAsync(Guid userId);
    Task<TestResultDto?> GetResultByIdAsync(Guid userId, Guid attemptId);
    Task<List<QuestionDto>> GetReinforcementQuestionsAsync(CefrLevel level, SkillArea skill);
    Task<TestResultDto> SubmitReinforcementAsync(Guid userId, CefrLevel level, SkillArea skill, List<SubmitAnswerDto> answers);

    // Generates up to `count` new questions via Groq and persists the
    // ones that pass validation (see IAiQuestionGeneratorService) as
    // real Questions, so they grade through the normal submit path.
    // Can return fewer than `count` (even zero) if generation fails for
    // some/all - never throws for a bad generation, only for a real
    // infrastructure failure.
    Task<List<QuestionDto>> GenerateReinforcementQuestionsAsync(CefrLevel level, SkillArea skill, int count);
}
