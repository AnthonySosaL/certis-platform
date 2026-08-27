namespace EnglishC1.Client.Infrastructure.Ai;

// Bound from configuration section "Groq". ApiKey lives only in dotnet
// user-secrets locally / the deployed web.config's environment variables
// in production - never in a committed appsettings*.json (same pattern
// as Jwt:SigningKey - see JwtOptions).
//
// Deliberately just the config slot for now - no HttpClient registration,
// no service that calls the Groq API yet. Nothing in the app reads
// GroqOptions yet either. Wiring an actual AI feature (what it does, what
// it costs per call, what happens when the key is missing/rate-limited)
// needs a real decision first, not a speculative build - see
// docs/PENDING_IDEAS.md.
public class GroqOptions
{
    public const string SectionName = "Groq";

    public string? ApiKey { get; init; }
}
