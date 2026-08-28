using EnglishC1.Client.Domain.PlacementTest;

namespace EnglishC1.Client.Domain.Tests.PlacementTest;

public class PlacementScorerTests
{
    // Every (level, skill) cell gets `correctCount` correct answers out of 4 total.
    private static (List<Question> Questions, List<TestAnswer> Answers) BuildAttempt(
        Dictionary<(CefrLevel Level, SkillArea Skill), int> correctCountByCell)
    {
        var questions = new List<Question>();
        var answers = new List<TestAnswer>();

        foreach (var ((level, skill), correctCount) in correctCountByCell)
        {
            for (var i = 0; i < 4; i++)
            {
                var correctOptionId = Guid.NewGuid();
                var wrongOptionId = Guid.NewGuid();
                var question = new Question
                {
                    Id = Guid.NewGuid(),
                    Text = $"{level} {skill} Q{i}",
                    Level = level,
                    SkillArea = skill,
                    CorrectOptionId = correctOptionId,
                    Options = [new QuestionOption { Id = correctOptionId, Text = "correct" }, new QuestionOption { Id = wrongOptionId, Text = "wrong" }],
                };
                questions.Add(question);

                var isCorrect = i < correctCount;
                answers.Add(new TestAnswer
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    SelectedOptionId = isCorrect ? correctOptionId : wrongOptionId,
                    IsCorrect = isCorrect,
                });
            }
        }

        return (questions, answers);
    }

    private static List<SkillBreakdown> Score(Dictionary<(CefrLevel, SkillArea), int> correctCountByCell)
    {
        var (questions, answers) = BuildAttempt(correctCountByCell);
        var questionsById = questions.ToDictionary(q => q.Id);
        return PlacementScorer.BuildBreakdown(answers, questionsById);
    }

    [Fact]
    public void AllCellsPassing_PlacesAtHighestLevel()
    {
        var breakdown = Score(new()
        {
            [(CefrLevel.A2, SkillArea.Grammar)] = 4,
            [(CefrLevel.A2, SkillArea.Vocabulary)] = 4,
            [(CefrLevel.B1, SkillArea.Grammar)] = 3,
            [(CefrLevel.B1, SkillArea.Vocabulary)] = 3,
            [(CefrLevel.B2, SkillArea.Grammar)] = 3,
            [(CefrLevel.B2, SkillArea.Vocabulary)] = 3,
            [(CefrLevel.C1, SkillArea.Grammar)] = 3,
            [(CefrLevel.C1, SkillArea.Vocabulary)] = 3,
        });

        Assert.Equal(CefrLevel.C1, PlacementScorer.ComputePlacement(breakdown));
        Assert.All(breakdown, cell => Assert.False(cell.NeedsReinforcement));
    }

    [Fact]
    public void WeakA2Cell_PlacesBelowA2EvenWithStrongHigherLevels()
    {
        // A2 grammar fails (1/4 = 25%), everything above is perfect - the
        // "consecutive from A2" rule should still cap placement at null
        // ("below A2"), not skip ahead to reward the strong upper levels.
        var breakdown = Score(new()
        {
            [(CefrLevel.A2, SkillArea.Grammar)] = 1,
            [(CefrLevel.A2, SkillArea.Vocabulary)] = 4,
            [(CefrLevel.B1, SkillArea.Grammar)] = 4,
            [(CefrLevel.B1, SkillArea.Vocabulary)] = 4,
            [(CefrLevel.C1, SkillArea.Grammar)] = 4,
            [(CefrLevel.C1, SkillArea.Vocabulary)] = 4,
        });

        Assert.Null(PlacementScorer.ComputePlacement(breakdown));

        var weakCell = breakdown.Single(b => b is { Level: CefrLevel.A2, SkillArea: SkillArea.Grammar });
        Assert.True(weakCell.NeedsReinforcement);
    }

    [Fact]
    public void PassesA2AndB1ButFailsB2_PlacesAtB1()
    {
        var breakdown = Score(new()
        {
            [(CefrLevel.A2, SkillArea.Grammar)] = 4,
            [(CefrLevel.A2, SkillArea.Vocabulary)] = 4,
            [(CefrLevel.B1, SkillArea.Grammar)] = 3,
            [(CefrLevel.B1, SkillArea.Vocabulary)] = 3,
            [(CefrLevel.B2, SkillArea.Grammar)] = 1,
            [(CefrLevel.B2, SkillArea.Vocabulary)] = 4,
            [(CefrLevel.C1, SkillArea.Grammar)] = 4,
            [(CefrLevel.C1, SkillArea.Vocabulary)] = 4,
        });

        Assert.Equal(CefrLevel.B1, PlacementScorer.ComputePlacement(breakdown));

        var weakCell = breakdown.Single(b => b is { Level: CefrLevel.B2, SkillArea: SkillArea.Grammar });
        Assert.True(weakCell.NeedsReinforcement);
        var strongCell = breakdown.Single(b => b is { Level: CefrLevel.C1, SkillArea: SkillArea.Grammar });
        Assert.False(strongCell.NeedsReinforcement); // still flagged as solid on its own, just doesn't raise the placement
    }

    [Theory]
    [InlineData(3, false)] // 3/4 = 75% >= 70% -> passes
    [InlineData(2, true)] // 2/4 = 50% < 70% -> needs reinforcement
    public void SeventyPercentThreshold_IsAppliedPerCell(int correctCount, bool expectedNeedsReinforcement)
    {
        var breakdown = Score(new() { [(CefrLevel.A2, SkillArea.Grammar)] = correctCount });

        Assert.Equal(expectedNeedsReinforcement, breakdown.Single().NeedsReinforcement);
    }

    [Theory]
    [InlineData(4, 10.0)]
    [InlineData(3, 7.5)]
    [InlineData(2, 5.0)]
    [InlineData(0, 0.0)]
    public void Grade_IsOutOfTenRoundedToOneDecimal(int correctCount, double expectedGrade)
    {
        var breakdown = Score(new() { [(CefrLevel.A2, SkillArea.Grammar)] = correctCount });

        Assert.Equal(expectedGrade, breakdown.Single().Grade);
    }

    [Fact]
    public void NoAnswers_ReturnsEmptyBreakdownAndNullPlacement()
    {
        var breakdown = PlacementScorer.BuildBreakdown([], new Dictionary<Guid, Question>());

        Assert.Empty(breakdown);
        Assert.Null(PlacementScorer.ComputePlacement(breakdown));
    }
}
