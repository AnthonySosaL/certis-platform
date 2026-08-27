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
        var result = new List<AccountDto>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new AccountDto(user.Id, user.Email!, roles.Contains("Admin"), roles.Contains("Tutor")));
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

        return Ok(new AccountDto(user.Id, user.Email!, request.IsAdmin, request.IsTutor));
    }

    private async Task ReconcileRole(ApplicationUser user, string role, bool shouldHaveRole)
    {
        var hasRole = await userManager.IsInRoleAsync(user, role);
        if (shouldHaveRole && !hasRole) await userManager.AddToRoleAsync(user, role);
        if (!shouldHaveRole && hasRole) await userManager.RemoveFromRoleAsync(user, role);
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
}
