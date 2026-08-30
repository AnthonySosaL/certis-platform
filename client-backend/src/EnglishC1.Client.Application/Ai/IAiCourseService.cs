using EnglishC1.Client.Domain.PlacementTest;

namespace EnglishC1.Client.Application.Ai;

// A real, multi-slide course (not the one-paragraph pre-quiz lesson) for
// one specific (level, skill) combination - 2026-08-29, explicitly
// requested: "de ley debe haber un curso con algo de texto, imagenes,
// ideas... segun sea grammar... y en su nivel respectivo". Generated on
// demand via Groq, not persisted - regenerating on request is simpler
// than adding storage for content this cheap to produce.
public interface IAiCourseService
{
    Task<List<CourseSlideDto>?> GenerateCourseAsync(CefrLevel level, SkillArea skill, CancellationToken ct = default);
}
