using EnglishC1.Client.Application.Speaking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishC1.Client.Api.Speaking;

// AI-only roleplay practice (2026-08-28) - the first buildable slice of
// "speaking practice, Cambridge-exam style". Matching with another real
// user is a separate, materially bigger feature (presence, pairing,
// likely real-time) deliberately not attempted here - see
// docs/PENDING_IDEAS.md.
[ApiController]
[Route("api/speaking")]
[Authorize]
public class SpeakingController(ISpeakingService speakingService) : ControllerBase
{
    [HttpGet("scenarios")]
    public ActionResult<List<SpeakingScenarioDto>> GetScenarios() => Ok(speakingService.GetScenarios());

    [HttpPost("{scenarioId}/reply")]
    public async Task<ActionResult<SpeakingReplyDto>> Reply(string scenarioId, SpeakingReplyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { message = "Message can't be empty." });

        if (!speakingService.GetScenarios().Any(s => s.Id == scenarioId))
            return NotFound();

        var reply = await speakingService.GenerateReplyAsync(scenarioId, request.History, request.Message);
        if (reply is null)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Speaking practice isn't available right now." });

        return Ok(new SpeakingReplyDto(reply));
    }
}
