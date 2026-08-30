namespace EnglishC1.Client.Application.Ai;

public record CourseSlideDto(string Title, string Body);

public record CourseDto(List<CourseSlideDto> Slides);
