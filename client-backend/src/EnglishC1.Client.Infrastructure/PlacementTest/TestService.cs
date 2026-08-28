using EnglishC1.Client.Application.PlacementTest;
using EnglishC1.Client.Domain.PlacementTest;
using EnglishC1.Client.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishC1.Client.Infrastructure.PlacementTest;

// Scoring model (fixed-form, not adaptive - see docs/PENDING_IDEAS.md for
// why): every placement attempt answers a fixed 32 questions (4 per
// (level, skill) cell) in one sitting. The actual scoring/placement
// algorithm lives in EnglishC1.Client.Domain.PlacementTest.PlacementScorer
// (pure, unit tested) - this class is just the EF Core plumbing around
// it: load questions, grade answers, persist the attempt, map to DTOs.
public class TestService(AppDbContext db) : ITestService
{
    // The bank now holds more than 4 questions per cell (see
    // QuestionSeeder) so repeat placement attempts don't always show the
    // exact same 32 questions - but the placement test itself deliberately
    // stays fixed-length rather than growing with the bank; a longer bank
    // buys variety and a bigger reinforcement pool, not a longer test.
    private const int PlacementQuestionsPerCell = 4;

    public async Task<List<QuestionDto>> GetPlacementQuestionsAsync()
    {
        var questions = await db.Questions.Include(q => q.Options).ToListAsync();
        var sampled = questions
            .GroupBy(q => (q.Level, q.SkillArea))
            .SelectMany(cell => cell.OrderBy(_ => Random.Shared.Next()).Take(PlacementQuestionsPerCell));
        return sampled.Select(ToDto).ToList();
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
        var (attempt, breakdown, questionsById) = Grade(userId, AttemptKind.Placement, questions, answers, focusLevel: null, focusSkill: null);

        attempt.PlacementResult = PlacementScorer.ComputePlacement(breakdown);

        db.TestAttempts.Add(attempt);
        await db.SaveChangesAsync();

        return ToResultDto(attempt, breakdown, questionsById);
    }

    public async Task<TestResultDto> SubmitReinforcementAsync(Guid userId, CefrLevel level, SkillArea skill, List<SubmitAnswerDto> answers)
    {
        var questions = await db.Questions
            .Include(q => q.Options)
            .Where(q => q.Level == level && q.SkillArea == skill)
            .ToListAsync();
        var (attempt, breakdown, questionsById) = Grade(userId, AttemptKind.Reinforcement, questions, answers, focusLevel: level, focusSkill: skill);

        db.TestAttempts.Add(attempt);
        await db.SaveChangesAsync();

        return ToResultDto(attempt, breakdown, questionsById);
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
        var questionsById = await db.Questions
            .Include(q => q.Options)
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);
        var breakdown = PlacementScorer.BuildBreakdown(attempt.Answers, questionsById);

        return ToResultDto(attempt, breakdown, questionsById);
    }

    public async Task<List<TestResultDto>> GetHistoryAsync(Guid userId)
    {
        var attempts = await db.TestAttempts
            .Include(a => a.Answers)
            .Where(a => a.UserId == userId && a.CompletedAtUtc != null)
            .OrderByDescending(a => a.CompletedAtUtc)
            .ToListAsync();

        if (attempts.Count == 0) return [];

        // One batched question lookup for every attempt in the history,
        // instead of a query per attempt.
        var allQuestionIds = attempts.SelectMany(a => a.Answers.Select(ans => ans.QuestionId)).Distinct().ToList();
        var questionsById = await db.Questions
            .Include(q => q.Options)
            .Where(q => allQuestionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        return attempts
            .Select(attempt => ToResultDto(attempt, PlacementScorer.BuildBreakdown(attempt.Answers, questionsById), questionsById))
            .ToList();
    }

    public async Task<TestResultDto?> GetResultByIdAsync(Guid userId, Guid attemptId)
    {
        var attempt = await db.TestAttempts
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId && a.CompletedAtUtc != null);

        if (attempt is null) return null;

        var questionIds = attempt.Answers.Select(a => a.QuestionId).ToList();
        var questionsById = await db.Questions
            .Include(q => q.Options)
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);
        var breakdown = PlacementScorer.BuildBreakdown(attempt.Answers, questionsById);

        return ToResultDto(attempt, breakdown, questionsById);
    }

    private static (TestAttempt Attempt, List<SkillBreakdown> Breakdown, Dictionary<Guid, Question> QuestionsById) Grade(
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
            // testAnswers.Count, not questions.Count: `questions` here is
            // whatever the caller loaded to grade against (the whole bank,
            // a superset), not what the test-taker was actually shown -
            // those only coincided by luck before GetPlacementQuestionsAsync
            // started sampling a fixed 32 out of a larger bank.
            TotalQuestions = testAnswers.Count,
        };
        foreach (var testAnswer in testAnswers)
        {
            testAnswer.TestAttemptId = attempt.Id;
            attempt.Answers.Add(testAnswer);
        }

        return (attempt, PlacementScorer.BuildBreakdown(testAnswers, questionsById), questionsById);
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

    private static TestResultDto ToResultDto(TestAttempt attempt, List<SkillBreakdown> breakdown, Dictionary<Guid, Question> questionsById) => new(
        attempt.Id,
        attempt.Kind,
        attempt.Score,
        attempt.TotalQuestions,
        attempt.TotalQuestions > 0 ? Math.Round((double)attempt.Score / attempt.TotalQuestions * 10, 1) : 0,
        attempt.PlacementResult,
        attempt.CompletedAtUtc ?? attempt.StartedAtUtc,
        breakdown
            .Select(b => new SkillBreakdownDto(b.Level, b.SkillArea, b.Correct, b.Total, b.NeedsReinforcement, b.Grade))
            .ToList(),
        BuildMissedQuestions(attempt.Answers, questionsById));

    private static List<MissedQuestionDto> BuildMissedQuestions(IEnumerable<TestAnswer> answers, Dictionary<Guid, Question> questionsById)
    {
        var missed = new List<MissedQuestionDto>();

        foreach (var answer in answers.Where(a => !a.IsCorrect))
        {
            if (!questionsById.TryGetValue(answer.QuestionId, out var question)) continue;

            var yourOption = question.Options.FirstOrDefault(o => o.Id == answer.SelectedOptionId);
            var correctOption = question.Options.FirstOrDefault(o => o.Id == question.CorrectOptionId);
            if (yourOption is null || correctOption is null) continue;

            missed.Add(new MissedQuestionDto(
                question.Id,
                question.Text,
                yourOption.Text,
                correctOption.Text,
                question.Explanation));
        }

        return missed;
    }
}
