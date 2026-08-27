namespace EnglishC1.Client.Infrastructure.Identity;

// Bound from configuration section "Jwt". SigningKey lives only in
// dotnet user-secrets locally / the deployed web.config's environment
// variables in production - never in a committed appsettings*.json.
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string SigningKey { get; init; }
    public int ExpirationMinutes { get; init; } = 60 * 24 * 7; // 7 days
}
