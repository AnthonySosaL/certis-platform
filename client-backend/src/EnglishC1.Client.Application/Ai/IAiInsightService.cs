using EnglishC1.Client.Application.PlacementTest;

namespace EnglishC1.Client.Application.Ai;

// Returns null when AI insight isn't available (no API key configured,
// or the upstream call failed) - the caller maps that to a clear "not
// available right now" response instead of a 500.
public interface IAiInsightService
{
    Task<string?> GenerateInsightAsync(TestResultDto result, CancellationToken ct = default);

    // Same idea, but across a student's FULL history (every placement +
    // reinforcement attempt) instead of one - a "what should I focus on
    // overall" panel rather than a per-attempt diagnostic. See
    // Dashboard's "Coach" panel, 2026-08-29.
    Task<string?> GenerateOverallInsightAsync(List<TestResultDto> history, CancellationToken ct = default);
}
