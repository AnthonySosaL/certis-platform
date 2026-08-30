using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnglishC1.Client.Application.Ai;
using EnglishC1.Client.Domain.PlacementTest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishC1.Client.Infrastructure.Ai;

// Generates a real multi-slide course for one (level, skill), via Groq's
// structured JSON output - same reliability reasoning as
// GroqQuestionGeneratorService (asking a reasoning model to "reply with
// JSON" in free text is much less reliable than response_format:
// json_object).
public class GroqCourseService(HttpClient http, IOptions<GroqOptions> options, ILogger<GroqCourseService> logger)
    : IAiCourseService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
    private const string Model = "openai/gpt-oss-20b";

    private const string SystemPrompt =
        "You are an English teacher writing a short course for Certis, an English placement and " +
        "practice platform. Given a CEFR level and a skill area, write a course of 4 to 6 slides that " +
        "teaches the key concepts a student needs for that level and skill, building up from simpler to " +
        "more advanced points, with at least one worked example per slide. Respond with ONLY a JSON " +
        "object matching this exact shape: {\"slides\": array of objects, each " +
        "{\"title\": string (short, a few words), \"body\": string (2-4 sentences, plain text, no " +
        "markdown)}}. No extra text outside the JSON object.";

    // One retry, same reasoning as the question generator - a reasoning
    // model occasionally returns something that parses as JSON but fails
    // validation (too few slides, an empty title).
    public async Task<List<CourseSlideDto>?> GenerateCourseAsync(CefrLevel level, SkillArea skill, CancellationToken ct = default)
    {
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey)) return null;

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var result = await TryGenerateOnceAsync(level, skill, ct);
            if (result is not null) return result;
        }

        return null;
    }

    private async Task<List<CourseSlideDto>?> TryGenerateOnceAsync(CefrLevel level, SkillArea skill, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);
            request.Content = JsonContent.Create(
                new GroqChatRequest(
                    Model,
                    [
                        new GroqChatMessage("system", SystemPrompt),
                        new GroqChatMessage("user", $"Level: {level}. Skill: {skill}. Write the course now."),
                    ],
                    Temperature: 0.7,
                    MaxTokens: 900,
                    ReasoningEffort: "low",
                    ResponseFormat: new GroqResponseFormat("json_object")),
                options: JsonOptions);

            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Groq course-generation request failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<GroqChatResponse>(JsonOptions, ct);
            var raw = payload?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(raw)) return null;

            var course = JsonSerializer.Deserialize<GeneratedCourseJson>(raw, JsonOptions);
            return Validate(course);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Groq course-generation attempt threw");
            return null;
        }
    }

    private static List<CourseSlideDto>? Validate(GeneratedCourseJson? candidate)
    {
        if (candidate?.Slides is not { Count: >= 2 } slides) return null;
        if (slides.Any(s => string.IsNullOrWhiteSpace(s.Title) || string.IsNullOrWhiteSpace(s.Body))) return null;

        return slides.Select(s => new CourseSlideDto(s.Title!.Trim(), s.Body!.Trim())).ToList();
    }

    private record GroqChatRequest(
        string Model,
        List<GroqChatMessage> Messages,
        double Temperature,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        [property: JsonPropertyName("reasoning_effort")] string ReasoningEffort,
        [property: JsonPropertyName("response_format")] GroqResponseFormat ResponseFormat);

    private record GroqResponseFormat(string Type);

    private record GroqChatMessage(string Role, string Content);

    private record GroqChatResponse(List<GroqChoice>? Choices);

    private record GroqChoice(GroqMessage? Message);

    private record GroqMessage(string? Content);

    private record GeneratedCourseJson(List<GeneratedSlideJson>? Slides);

    private record GeneratedSlideJson(string? Title, string? Body);
}
