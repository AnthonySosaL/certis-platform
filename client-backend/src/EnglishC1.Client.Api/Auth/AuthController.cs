using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using EnglishC1.Client.Infrastructure.Identity;

namespace EnglishC1.Client.Api.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return ValidationProblem(BuildErrorModelState(result));

        return Ok(await BuildAuthResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(await BuildAuthResponse(user));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() =>
        Ok(new { email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") });

    private async Task<AuthResponse> BuildAuthResponse(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new AuthResponse(
            Token: tokenService.CreateToken(user, roles),
            Email: user.Email!,
            ExpiresAtUtc: DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes),
            IsAdmin: roles.Contains("Admin"),
            IsTutor: roles.Contains("Tutor"));
    }

    private static ModelStateDictionary BuildErrorModelState(IdentityResult result)
    {
        var modelState = new ModelStateDictionary();
        foreach (var error in result.Errors)
            modelState.AddModelError(error.Code, error.Description);
        return modelState;
    }
}
