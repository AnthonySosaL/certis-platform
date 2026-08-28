namespace EnglishC1.Client.Application.Speaking;

public interface ISpeakingService
{
    List<SpeakingScenarioDto> GetScenarios();

    // Null if the scenario doesn't exist, the Groq call failed, or it
    // isn't configured - the controller maps that to a clear response
    // rather than a generic 500, same pattern as the other AI features.
    Task<string?> GenerateReplyAsync(string scenarioId, List<SpeakingTurnDto> history, string message, CancellationToken ct = default);
}
