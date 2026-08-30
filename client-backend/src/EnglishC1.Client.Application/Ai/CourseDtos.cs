namespace EnglishC1.Client.Application.Ai;

// Type is "content" (Title/Body only), "drag" (Prompt/Options/Answer -
// student drags the correct option into a blank), or "write"
// (Prompt/Answer - student types the answer). Drag/write exercises are
// ungraded self-checks woven into the course (2026-08-30, explicitly
// requested: "eso no tiene nota ni nada es para que vea si comprendio")
// - safe to send Answer to the client since nothing here is persisted
// or scored server-side, unlike the real reinforcement quiz.
public record CourseSlideDto(
    string Type,
    string? Title,
    string? Body,
    string? Prompt,
    List<string>? Options,
    string? Answer);

public record CourseDto(List<CourseSlideDto> Slides);
