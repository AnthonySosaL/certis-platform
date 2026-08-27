namespace EnglishC1.Client.Domain.PlacementTest;

// Only the bands the placement test actually covers - A1 and C2 aren't
// tested for yet (no question bank for them). "Below A2" is represented
// as a null CefrLevel result, not a fifth enum value.
public enum CefrLevel
{
    A2,
    B1,
    B2,
    C1,
}
