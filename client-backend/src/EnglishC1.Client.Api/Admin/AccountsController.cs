using System.Security.Claims;
using EnglishC1.Client.Application.Admin;
using EnglishC1.Client.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EnglishC1.Client.Api.Admin;

// Admin-only: who gets to see/edit platform data. There's no self-service
// "become a tutor" flow on purpose - an account only gains Admin or Tutor
// through another Admin explicitly granting it here.
[ApiController]
[Route("api/admin/accounts")]
[Authorize(Roles = "Admin")]
public class AccountsController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAccounts()
    {
        var users = userManager.Users.ToList();
        var labelById = users.ToDictionary(u => u.Id, u => u.DisplayName ?? u.Email!);

        var result = new List<AccountDto>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var tutorLabel = user.TutorId.HasValue && labelById.TryGetValue(user.TutorId.Value, out var label) ? label : null;
            result.Add(new AccountDto(user.Id, user.Email!, roles.Contains("Admin"), roles.Contains("Tutor"), user.TutorId, tutorLabel));
        }
        return Ok(result.OrderBy(a => a.Email).ToList());
    }

    [HttpPut("{userId:guid}/roles")]
    public async Task<ActionResult<AccountDto>> SetRoles(Guid userId, SetRolesRequest request)
    {
        if (userId == CurrentUserId && !request.IsAdmin)
            return BadRequest(new { message = "You can't remove your own Admin role." });

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return NotFound();

        await ReconcileRole(user, "Admin", request.IsAdmin);
        await ReconcileRole(user, "Tutor", request.IsTutor);

        return Ok(new AccountDto(user.Id, user.Email!, request.IsAdmin, request.IsTutor, user.TutorId, await ResolveTutorLabel(user)));
    }

    [HttpPut("{userId:guid}/tutor")]
    public async Task<ActionResult<AccountDto>> SetTutor(Guid userId, SetTutorRequest request)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return NotFound();

        if (request.TutorUserId == userId)
            return BadRequest(new { message = "An account can't be its own tutor." });

        string? tutorLabel = null;
        if (request.TutorUserId.HasValue)
        {
            var tutor = await userManager.FindByIdAsync(request.TutorUserId.Value.ToString());
            if (tutor is null) return BadRequest(new { message = "That tutor account doesn't exist." });
            if (!await userManager.IsInRoleAsync(tutor, "Tutor"))
                return BadRequest(new { message = "That account doesn't have the Tutor role." });
            tutorLabel = tutor.DisplayName ?? tutor.Email;
        }

        user.TutorId = request.TutorUserId;
        await userManager.UpdateAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new AccountDto(user.Id, user.Email!, roles.Contains("Admin"), roles.Contains("Tutor"), user.TutorId, tutorLabel));
    }

    private async Task ReconcileRole(ApplicationUser user, string role, bool shouldHaveRole)
    {
        var hasRole = await userManager.IsInRoleAsync(user, role);
        if (shouldHaveRole && !hasRole) await userManager.AddToRoleAsync(user, role);
        if (!shouldHaveRole && hasRole) await userManager.RemoveFromRoleAsync(user, role);
    }

    private async Task<string?> ResolveTutorLabel(ApplicationUser user)
    {
        if (!user.TutorId.HasValue) return null;
        var tutor = await userManager.FindByIdAsync(user.TutorId.Value.ToString());
        return tutor?.DisplayName ?? tutor?.Email;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
}
