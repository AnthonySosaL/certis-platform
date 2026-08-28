using EnglishC1.Client.Application.Admin;
using EnglishC1.Client.Domain.PlacementTest;
using EnglishC1.Client.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishC1.Client.Infrastructure.Admin;

public class ContentService(AppDbContext db) : IContentService
{
    public async Task<List<AdminQuestionDto>> GetQuestionsAsync()
    {
        var questions = await db.Questions
            .Include(q => q.Options)
            .OrderBy(q => q.Level)
            .ThenBy(q => q.SkillArea)
            .ThenBy(q => q.Text)
            .ToListAsync();

        return questions.Select(ToDto).ToList();
    }

    public async Task<AdminQuestionDto> CreateQuestionAsync(UpsertQuestionRequest request)
    {
        var question = BuildQuestion(Guid.NewGuid(), request);
        db.Questions.Add(question);
        await db.SaveChangesAsync();
        return ToDto(question);
    }

    public async Task<AdminQuestionDto?> UpdateQuestionAsync(Guid id, UpsertQuestionRequest request)
    {
        var existing = await db.Questions.Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == id);
        if (existing is null) return null;

        // Options don't have a stable identity worth preserving across an
        // edit (the client always resubmits the full list) - simplest to
        // replace them wholesale rather than diff and patch in place.
        //
        // AddRange below is required, not optional: assigning a plain
        // List<QuestionOption> of freshly-constructed entities to
        // existing.Options (an already-tracked parent's navigation
        // property) does NOT mark them Added. Because their Id is a
        // client-set Guid rather than DB-generated, EF's graph fixup
        // assumes they already exist and queues no-op UPDATEs instead of
        // INSERTs, which then throw DbUpdateConcurrencyException (0 rows
        // affected) - caught this via a real edit through the admin UI.
        db.QuestionOptions.RemoveRange(existing.Options);
        var newOptions = request.Options
            .Select(text => new QuestionOption { Id = Guid.NewGuid(), QuestionId = id, Text = text })
            .ToList();
        db.QuestionOptions.AddRange(newOptions);

        existing.Text = request.Text;
        existing.Level = request.Level;
        existing.SkillArea = request.SkillArea;
        existing.Explanation = request.Explanation;
        existing.Passage = request.Passage;
        existing.AudioUrl = request.AudioUrl;
        existing.Options = newOptions;
        existing.CorrectOptionId = newOptions[request.CorrectOptionIndex].Id;

        await db.SaveChangesAsync();
        return ToDto(existing);
    }

    public async Task<bool> DeleteQuestionAsync(Guid id)
    {
        var existing = await db.Questions.FirstOrDefaultAsync(q => q.Id == id);
        if (existing is null) return false;

        db.Questions.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }

    private static Question BuildQuestion(Guid id, UpsertQuestionRequest request)
    {
        var question = new Question
        {
            Id = id,
            Text = request.Text,
            Level = request.Level,
            SkillArea = request.SkillArea,
            Explanation = request.Explanation,
            Passage = request.Passage,
            AudioUrl = request.AudioUrl,
        };
        question.Options = request.Options
            .Select(text => new QuestionOption { Id = Guid.NewGuid(), QuestionId = id, Text = text })
            .ToList();
        question.CorrectOptionId = question.Options[request.CorrectOptionIndex].Id;
        return question;
    }

    private static AdminQuestionDto ToDto(Question q) => new(
        q.Id,
        q.Text,
        q.Level,
        q.SkillArea,
        q.Explanation,
        q.Options.Select(o => new AdminQuestionOptionDto(o.Id, o.Text)).ToList(),
        q.CorrectOptionId,
        q.IsAiGenerated,
        q.Passage,
        q.AudioUrl);
}
