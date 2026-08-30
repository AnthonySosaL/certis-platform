using EnglishC1.Client.Domain.PlacementTest;

namespace EnglishC1.Client.Application.Ai;

// Optional "take a quick lesson first" step before a reinforcement quiz
// (2026-08-29) - explicitly requested: practicing via Courses/reinforcement
// was "just more tests" with nowhere to actually learn the material
// first. Returns null when unavailable (no API key, or the upstream
// call failed), same pattern as IAiInsightService.
public interface IAiLessonService
{
    Task<string?> GenerateLessonAsync(CefrLevel level, SkillArea skill, CancellationToken ct = default);
}
