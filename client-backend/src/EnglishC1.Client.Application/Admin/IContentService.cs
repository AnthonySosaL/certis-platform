namespace EnglishC1.Client.Application.Admin;

// "Modules" in the product sense are (Level, SkillArea) cells - this
// service manages the individual questions inside them. Adding a new
// SkillArea (Reading, Listening) is a bigger change (new enum member,
// new reinforcement routing) and stays out of scope here - see
// docs/PENDING_IDEAS.md; this CRUD works with whatever skills exist.
public interface IContentService
{
    Task<List<AdminQuestionDto>> GetQuestionsAsync();
    Task<AdminQuestionDto> CreateQuestionAsync(UpsertQuestionRequest request);
    Task<AdminQuestionDto?> UpdateQuestionAsync(Guid id, UpsertQuestionRequest request);
    Task<bool> DeleteQuestionAsync(Guid id);
}
