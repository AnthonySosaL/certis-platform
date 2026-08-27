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
}
