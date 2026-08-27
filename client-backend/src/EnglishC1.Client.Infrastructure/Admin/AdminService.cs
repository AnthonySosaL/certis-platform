using EnglishC1.Client.Application.Admin;
using EnglishC1.Client.Domain.PlacementTest;
using EnglishC1.Client.Infrastructure.Identity;
using EnglishC1.Client.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnglishC1.Client.Infrastructure.Admin;

// Early-warning signal: for every (level, skill) cell a student's latest
// placement flagged as NeedsReinforcement, has there been a *passing*
// reinforcement attempt for that exact cell since? If not, it's still an
// open weak area - that's what drives HasEarlyWarning. A student who
// hasn't taken the placement test at all isn't flagged (nothing to warn
// about yet, not a red flag on day one).
public class AdminService(AppDbContext db, UserManager<ApplicationUser> userManager) : IAdminService
{
    public async Task<List<StudentSummaryDto>> GetStudentSummariesAsync()
    {
        var users = await userManager.Users.ToListAsync();

        var attempts = await db.TestAttempts
            .Include(a => a.Answers)
            .Where(a => a.CompletedAtUtc != null)
            .ToListAsync();

        var allQuestionIds = attempts.SelectMany(a => a.Answers.Select(ans => ans.QuestionId)).Distinct().ToList();
        var questionsById = await db.Questions
            .Include(q => q.Options)
            .Where(q => allQuestionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        var attemptsByUser = attempts.ToLookup(a => a.UserId);

        return users
            .Select(user => BuildSummary(user, attemptsByUser[user.Id].ToList(), questionsById))
            .OrderByDescending(s => s.HasEarlyWarning)
            .ThenBy(s => s.Email)
            .ToList();
    }

    private static StudentSummaryDto BuildSummary(
        ApplicationUser user,
        List<TestAttempt> userAttempts,
        Dictionary<Guid, Question> questionsById)
    {
        var latestPlacement = userAttempts
            .Where(a => a.Kind == AttemptKind.Placement)
            .OrderByDescending(a => a.CompletedAtUtc)
            .FirstOrDefault();

        if (latestPlacement is null)
        {
            return new StudentSummaryDto(user.Id, user.Email!, null, null, userAttempts.Count, [], HasEarlyWarning: false);
        }

        var breakdown = PlacementScorer.BuildBreakdown(latestPlacement.Answers, questionsById);
        var weakCells = breakdown.Where(b => b.NeedsReinforcement).ToList();

        var unresolved = weakCells
            .Where(cell => !userAttempts.Any(a =>
                a.Kind == AttemptKind.Reinforcement &&
                a.FocusLevel == cell.Level &&
                a.FocusSkill == cell.SkillArea &&
                a.CompletedAtUtc > latestPlacement.CompletedAtUtc &&
                a.TotalQuestions > 0 &&
                (double)a.Score / a.TotalQuestions >= PlacementScorer.PassThreshold))
            .Select(cell => new WeakAreaDto(cell.Level, cell.SkillArea))
            .ToList();

        return new StudentSummaryDto(
            user.Id,
            user.Email!,
            latestPlacement.PlacementResult,
            latestPlacement.CompletedAtUtc,
            userAttempts.Count,
            unresolved,
            HasEarlyWarning: unresolved.Count > 0);
    }
}
