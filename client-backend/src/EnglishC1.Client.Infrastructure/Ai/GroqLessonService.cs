using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnglishC1.Client.Application.Ai;
using EnglishC1.Client.Domain.PlacementTest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishC1.Client.Infrastructure.Ai;

// Generates the optional pre-quiz mini-lesson via Groq, on demand only -
// same "student clicks a button" cost/latency reasoning as
// GroqInsightService and GroqQuestionGeneratorService.
public class GroqLessonService(HttpClient http, IOptions<GroqOptions> options, ILogger<GroqLessonService> logger)
    : IAiLessonService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
    private const string Model = "openai/gpt-oss-20b";

    private const string SystemPrompt =
        "You are an English teacher for Certis, an English placement and practice platform. Write a " +
        "short, clear mini-lesson for a student about to practice one specific CEFR level and skill " +
        "combination. Explain the key rules or focus points they should know for that level and skill, " +
        "with one or two short examples woven naturally into the prose. Plain text only - no markdown, " +
        "no bullet points, no headers - just clear paragraphs a student can read in under a minute. " +
        "150-220 words. Address the student as \"you\".";

    public async Task<string?> GenerateLessonAsync(CefrLevel level, SkillArea skill, CancellationToken ct = default)
    {
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey)) return null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = JsonContent.Create(
                new GroqChatRequest(
                    Model,
                    [
                        new GroqChatMessage("system", SystemPrompt),
                        new GroqChatMessage("user", $"Level: {level}. Skill: {skill}. Write the mini-lesson now."),
                    ],
                    Temperature: 0.6,
                    MaxTokens: 350,
                    ReasoningEffort: "low"),
                options: JsonOptions);

            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Groq lesson request failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<GroqChatResponse>(JsonOptions, ct);
            var text = payload?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Groq lesson request threw");
            return null;
        }
    }

    private record GroqChatRequest(
        string Model,
        List<GroqChatMessage> Messages,
        double Temperature,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        [property: JsonPropertyName("reasoning_effort")] string ReasoningEffort);

    private record GroqChatMessage(string Role, string Content);

    private record GroqChatResponse(List<GroqChoice>? Choices);

    private record GroqChoice(GroqMessage? Message);

    private record GroqMessage(string? Content);
}
