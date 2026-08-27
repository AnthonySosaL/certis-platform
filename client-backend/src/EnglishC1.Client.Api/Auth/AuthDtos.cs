using System.ComponentModel.DataAnnotations;

namespace EnglishC1.Client.Api.Auth;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(string Token, string Email, DateTime ExpiresAtUtc);
