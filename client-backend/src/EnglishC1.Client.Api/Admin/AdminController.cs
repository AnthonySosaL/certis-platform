using EnglishC1.Client.Application.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishC1.Client.Api.Admin;

// Read-only student overview - both Admin and Tutor can see it. Granting
// roles (AccountsController) and editing question content
// (ContentController) have their own, separately-scoped authorization.
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,Tutor")]
public class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("students")]
    public async Task<ActionResult<List<StudentSummaryDto>>> GetStudents() =>
        Ok(await adminService.GetStudentSummariesAsync());
}
