using System.Security.Claims;
using EnglishC1.Client.Application.Ai;
using EnglishC1.Client.Application.PlacementTest;
using EnglishC1.Client.Domain.PlacementTest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishC1.Client.Api.PlacementTest;

[ApiController]
[Route("api/test")]
[Authorize]
public class TestController(ITestService testService, IAiInsightService aiInsightService) : ControllerBase
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

    // On-demand only - the student clicks a button for this, it's never
    // generated automatically. 503 (not 500) when it's unavailable: no
    // Groq:ApiKey configured yet, or the upstream call failed - both are
    // "try again later", not a server bug.
    [HttpPost("results/{attemptId:guid}/insight")]
    public async Task<ActionResult<TestInsightDto>> GetInsight(Guid attemptId)
    {
        var result = await testService.GetResultByIdAsync(UserId, attemptId);
        if (result is null) return NotFound();

        var insight = await aiInsightService.GenerateInsightAsync(result);
        if (insight is null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "AI feedback isn't available right now." });

        return Ok(new TestInsightDto(insight));
    }

    // Coach panel (2026-08-29) - "what should I improve overall", based
    // on every attempt on record, not one. Distinct from GetInsight
    // above (per-attempt) rather than replacing it - this one needs the
    // full history to exist first, so 404s if there's none yet.
    [HttpPost("insight/overall")]
    public async Task<ActionResult<TestInsightDto>> GetOverallInsight()
    {
        var history = await testService.GetHistoryAsync(UserId);
        if (history.Count == 0) return NotFound();

        var insight = await aiInsightService.GenerateOverallInsightAsync(history);
        if (insight is null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "AI feedback isn't available right now." });

        return Ok(new TestInsightDto(insight));
    }

    [HttpGet("reinforcement/{level}/{skill}/questions")]
    public async Task<ActionResult<List<QuestionDto>>> GetReinforcementQuestions(CefrLevel level, SkillArea skill) =>
        Ok(await testService.GetReinforcementQuestionsAsync(level, skill));

    [HttpPost("reinforcement/{level}/{skill}/submit")]
    public async Task<ActionResult<TestResultDto>> SubmitReinforcement(CefrLevel level, SkillArea skill, List<SubmitAnswerDto> answers) =>
        Ok(await testService.SubmitReinforcementAsync(UserId, level, skill, answers));

    // On-demand practice beyond the fixed bank - generates via Groq, not
    // automatic. Count is capped (not just clamped to something huge by
    // accident) since each one is a real Groq call.
    [HttpPost("reinforcement/{level}/{skill}/generate")]
    public async Task<ActionResult<List<QuestionDto>>> GenerateReinforcementQuestions(CefrLevel level, SkillArea skill, [FromQuery] int count = 4)
    {
        var clamped = Math.Clamp(count, 1, 6);
        var questions = await testService.GenerateReinforcementQuestionsAsync(level, skill, clamped);
        if (questions.Count == 0)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "AI-generated practice isn't available right now." });

        return Ok(questions);
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
}
