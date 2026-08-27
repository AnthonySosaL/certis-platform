using System.Security.Claims;
using EnglishC1.Client.Application.PlacementTest;
using EnglishC1.Client.Domain.PlacementTest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishC1.Client.Api.PlacementTest;

[ApiController]
[Route("api/test")]
[Authorize]
public class TestController(ITestService testService) : ControllerBase
{
    [HttpGet("placement/questions")]
    public async Task<ActionResult<List<QuestionDto>>> GetPlacementQuestions() =>
        Ok(await testService.GetPlacementQuestionsAsync());

    [HttpPost("placement/submit")]
    public async Task<ActionResult<TestResultDto>> SubmitPlacementTest(List<SubmitAnswerDto> answers) =>
        Ok(await testService.SubmitPlacementTestAsync(UserId, answers));

    [HttpGet("results/placement/latest")]
    public async Task<ActionResult<TestResultDto>> GetLatestPlacementResult()
    {
        var result = await testService.GetLatestResultAsync(UserId, AttemptKind.Placement);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("results/history")]
    public async Task<ActionResult<List<TestResultDto>>> GetHistory() =>
        Ok(await testService.GetHistoryAsync(UserId));

    [HttpGet("reinforcement/{level}/{skill}/questions")]
    public async Task<ActionResult<List<QuestionDto>>> GetReinforcementQuestions(CefrLevel level, SkillArea skill) =>
        Ok(await testService.GetReinforcementQuestionsAsync(level, skill));

    [HttpPost("reinforcement/{level}/{skill}/submit")]
    public async Task<ActionResult<TestResultDto>> SubmitReinforcement(CefrLevel level, SkillArea skill, List<SubmitAnswerDto> answers) =>
        Ok(await testService.SubmitReinforcementAsync(UserId, level, skill, answers));

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
}
