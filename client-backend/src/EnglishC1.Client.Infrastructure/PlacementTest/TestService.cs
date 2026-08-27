using EnglishC1.Client.Application.PlacementTest;
using EnglishC1.Client.Domain.PlacementTest;
using EnglishC1.Client.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishC1.Client.Infrastructure.PlacementTest;

// Scoring model (fixed-form, not adaptive - see docs/PENDING_IDEAS.md for
// why): every placement attempt answers all 32 questions in one sitting.
// The actual scoring/placement algorithm lives in
// EnglishC1.Client.Domain.PlacementTest.PlacementScorer (pure, unit
// tested) - this class is just the EF Core plumbing around it: load
// questions, grade answers, persist the attempt, map to DTOs.
public class TestService(AppDbContext db) : ITestService
{
    public async Task<List<QuestionDto>> GetPlacementQuestionsAsync()
    {
        var questions = await db.Questions.Include(q => q.Options).ToListAsync();
        return questions.Select(ToDto).ToList();
    }

    public async Task<List<QuestionDto>> GetReinforcementQuestionsAsync(CefrLevel level, SkillArea skill)
    {
        var questions = await db.Questions
            .Include(q => q.Options)
            .Where(q => q.Level == level && q.SkillArea == skill)
            .ToListAsync();
        return questions.Select(ToDto).ToList();
    }

    public async Task<TestResultDto> SubmitPlacementTestAsync(Guid userId, List<SubmitAnswerDto> answers)
    {
        var questions = await db.Questions.Include(q => q.Options).ToListAsync();
        var (attempt, breakdown) = Grade(userId, AttemptKind.Placement, questions, answers, focusLevel: null, focusSkill: null);

        attempt.PlacementResult = PlacementScorer.ComputePlacement(breakdown);

        db.TestAttempts.Add(attempt);
        await db.SaveChangesAsync();

        return ToResultDto(attempt, breakdown);
    }

    public async Task<TestResultDto> SubmitReinforcementAsync(Guid userId, CefrLevel level, SkillArea skill, List<SubmitAnswerDto> answers)
    {
        var questions = await db.Questions
            .Include(q => q.Options)
            .Where(q => q.Level == level && q.SkillArea == skill)
            .ToListAsync();
        var (attempt, breakdown) = Grade(userId, AttemptKind.Reinforcement, questions, answers, focusLevel: level, focusSkill: skill);

        db.TestAttempts.Add(attempt);
        await db.SaveChangesAsync();

        return ToResultDto(attempt, breakdown);
    }

    public async Task<TestResultDto?> GetLatestResultAsync(Guid userId, AttemptKind kind)
    {
        var attempt = await db.TestAttempts
            .Include(a => a.Answers)
            .Where(a => a.UserId == userId && a.Kind == kind && a.CompletedAtUtc != null)
            .OrderByDescending(a => a.CompletedAtUtc)
            .FirstOrDefaultAsync();

        if (attempt is null) return null;

        var questionIds = attempt.Answers.Select(a => a.QuestionId).ToList();
        var questionsById = await db.Questions.Where(q => questionIds.Contains(q.Id)).ToDictionaryAsync(q => q.Id);
        var breakdown = PlacementScorer.BuildBreakdown(attempt.Answers, questionsById);

        return ToResultDto(attempt, breakdown);
    }

    private static (TestAttempt Attempt, List<SkillBreakdown> Breakdown) Grade(
        Guid userId,
        AttemptKind kind,
        List<Question> questions,
        List<SubmitAnswerDto> answers,
        CefrLevel? focusLevel,
        SkillArea? focusSkill)
    {
        var questionsById = questions.ToDictionary(q => q.Id);
        var testAnswers = new List<TestAnswer>();

        foreach (var answer in answers)
        {
            if (!questionsById.TryGetValue(answer.QuestionId, out var question)) continue;

            testAnswers.Add(new TestAnswer
            {
                Id = Guid.NewGuid(),
                QuestionId = answer.QuestionId,
                SelectedOptionId = answer.SelectedOptionId,
                IsCorrect = answer.SelectedOptionId == question.CorrectOptionId,
            });
        }

        var attempt = new TestAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Kind = kind,
            FocusLevel = focusLevel,
            FocusSkill = focusSkill,
            StartedAtUtc = DateTime.UtcNow,
            CompletedAtUtc = DateTime.UtcNow,
            Score = testAnswers.Count(a => a.IsCorrect),
            TotalQuestions = questions.Count,
        };
        foreach (var testAnswer in testAnswers)
        {
            testAnswer.TestAttemptId = attempt.Id;
            attempt.Answers.Add(testAnswer);
        }

        return (attempt, PlacementScorer.BuildBreakdown(testAnswers, questionsById));
    }

    private static QuestionDto ToDto(Question question)
    {
        var shuffled = question.Options.OrderBy(_ => Random.Shared.Next()).ToList();
        return new QuestionDto(
            question.Id,
            question.Text,
            question.SkillArea,
            question.Level,
            shuffled.Select(o => new QuestionOptionDto(o.Id, o.Text)).ToList());
    }

    private static TestResultDto ToResultDto(TestAttempt attempt, List<SkillBreakdown> breakdown) => new(
        attempt.Id,
        attempt.Kind,
        attempt.Score,
        attempt.TotalQuestions,
        attempt.PlacementResult,
        attempt.CompletedAtUtc ?? attempt.StartedAtUtc,
        breakdown
            .Select(b => new SkillBreakdownDto(b.Level, b.SkillArea, b.Correct, b.Total, b.NeedsReinforcement))
            .ToList());
}
