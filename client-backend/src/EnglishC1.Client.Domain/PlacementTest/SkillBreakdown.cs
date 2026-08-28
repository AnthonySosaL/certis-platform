namespace EnglishC1.Client.Domain.PlacementTest;

public record SkillBreakdown(CefrLevel Level, SkillArea SkillArea, int Correct, int Total, bool NeedsReinforcement)
{
    // School-style grade out of 10, alongside the raw Correct/Total -
    // requested explicitly ("como en los colegios evalúan"), paired with
    // PlacementScorer.PassThreshold (0.7, i.e. a 7/10 pass bar).
    public double Grade => Total > 0 ? Math.Round((double)Correct / Total * 10, 1) : 0;
}
