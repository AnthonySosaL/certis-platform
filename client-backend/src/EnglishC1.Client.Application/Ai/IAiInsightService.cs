using EnglishC1.Client.Application.PlacementTest;

namespace EnglishC1.Client.Application.Ai;

// Returns null when AI insight isn't available (no API key configured,
// or the upstream call failed) - the caller maps that to a clear "not
// available right now" response instead of a 500.
public interface IAiInsightService
{
    Task<string?> GenerateInsightAsync(TestResultDto result, CancellationToken ct = default);
}
