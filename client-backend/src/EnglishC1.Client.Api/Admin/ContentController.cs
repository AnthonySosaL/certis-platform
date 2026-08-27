using EnglishC1.Client.Application.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishC1.Client.Api.Admin;

// Question-bank CRUD - both Admin and Tutor can author content. See
// IContentService for what a "module" means here (a Level x SkillArea
// cell's worth of questions).
[ApiController]
[Route("api/admin/questions")]
[Authorize(Roles = "Admin,Tutor")]
public class ContentController(IContentService contentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AdminQuestionDto>>> GetQuestions() =>
        Ok(await contentService.GetQuestionsAsync());

    [HttpPost]
    public async Task<ActionResult<AdminQuestionDto>> CreateQuestion(UpsertQuestionRequest request)
    {
        if (!IsValid(request, out var error)) return BadRequest(new { message = error });
        return Ok(await contentService.CreateQuestionAsync(request));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminQuestionDto>> UpdateQuestion(Guid id, UpsertQuestionRequest request)
    {
        if (!IsValid(request, out var error)) return BadRequest(new { message = error });
        var updated = await contentService.UpdateQuestionAsync(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteQuestion(Guid id) =>
        await contentService.DeleteQuestionAsync(id) ? NoContent() : NotFound();

    private static bool IsValid(UpsertQuestionRequest request, out string error)
    {
        if (string.IsNullOrWhiteSpace(request.Text)) { error = "Question text is required."; return false; }
        if (request.Options.Count < 2) { error = "At least 2 options are required."; return false; }
        if (request.Options.Any(string.IsNullOrWhiteSpace)) { error = "Options can't be empty."; return false; }
        if (request.CorrectOptionIndex < 0 || request.CorrectOptionIndex >= request.Options.Count)
        {
            error = "Correct option index is out of range.";
            return false;
        }
        error = string.Empty;
        return true;
    }
}
