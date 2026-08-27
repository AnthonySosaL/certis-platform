using EnglishC1.Client.Domain.PlacementTest;

namespace EnglishC1.Client.Application.Admin;

// One (level, skill) area a student still needs to clear - surfaced so
// the admin panel can show *what* needs attention, not just a flag.
public record WeakAreaDto(CefrLevel Level, SkillArea SkillArea);

public record StudentSummaryDto(
    Guid UserId,
    string Email,
    CefrLevel? LatestPlacementLevel,
    DateTime? LatestPlacementDate,
    int TotalAttempts,
    List<WeakAreaDto> WeakAreas,
    bool HasEarlyWarning);
