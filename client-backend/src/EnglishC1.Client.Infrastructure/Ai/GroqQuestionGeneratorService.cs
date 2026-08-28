using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnglishC1.Client.Application.Ai;
using EnglishC1.Client.Domain.PlacementTest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishC1.Client.Infrastructure.Ai;

// Generates one new CEFR-level multiple-choice question on demand, for
// reinforcement practice beyond the fixed hand-written bank. Uses Groq's
// structured JSON output (response_format: json_object) instead of
// free-form prose parsing - much more reliable than asking the model to
// "reply with JSON" in plain text and hoping it doesn't wrap it in
// markdown fences or add commentary.
public class GroqQuestionGeneratorService(HttpClient http, IOptions<GroqOptions> options, ILogger<GroqQuestionGeneratorService> logger)
    : IAiQuestionGeneratorService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
    private const string Model = "openai/gpt-oss-20b";

    private const string SystemPrompt =
        "You write CEFR-level English multiple-choice questions for an English placement platform. " +
        "Given a CEFR level and a skill area, write ONE new multiple-choice question, different from " +
        "common textbook examples. Respond with ONLY a JSON object matching this exact shape: " +
        "{\"text\": string (the question - use ___ for a blank if it's a grammar question), " +
        "\"options\": array of exactly 4 strings, \"correctIndex\": integer 0-3, " +
        "\"explanation\": string (one sentence explaining the grammar/vocabulary rule)}. " +
        "No markdown, no extra text, just the JSON object.";

    public async Task<GeneratedQuestion?> GenerateQuestionAsync(
        CefrLevel level,
        SkillArea skill,
        IReadOnlyCollection<string> avoidTexts,
        CancellationToken ct = default)
    {
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey)) return null;

        var userPrompt = BuildUserPrompt(level, skill, avoidTexts);

        // One retry: a reasoning model occasionally returns something
        // that parses as JSON but fails validation (wrong option count,
        // out-of-range index) - worth one more try before giving up,
        // rather than failing a practice request over a single bad roll.
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var result = await TryGenerateOnceAsync(userPrompt, ct);
            if (result is not null) return result;
        }

        return null;
    }

    private async Task<GeneratedQuestion?> TryGenerateOnceAsync(string userPrompt, CancellationToken ct)
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
                        new GroqChatMessage("user", userPrompt),
                    ],
                    Temperature: 0.9,
                    MaxTokens: 500,
                    ReasoningEffort: "low",
                    ResponseFormat: new GroqResponseFormat("json_object")),
                options: JsonOptions);

            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Groq question-generation request failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<GroqChatResponse>(JsonOptions, ct);
            var raw = payload?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(raw)) return null;

            var generated = JsonSerializer.Deserialize<GeneratedQuestionJson>(raw, JsonOptions);
            return Validate(generated);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Groq question-generation attempt threw");
            return null;
        }
    }

    private static GeneratedQuestion? Validate(GeneratedQuestionJson? candidate)
    {
        if (candidate is null) return null;
        if (string.IsNullOrWhiteSpace(candidate.Text)) return null;
        if (candidate.Options is not { Count: 4 }) return null;
        if (candidate.Options.Any(string.IsNullOrWhiteSpace)) return null;
        if (candidate.CorrectIndex is < 0 or > 3) return null;
        if (string.IsNullOrWhiteSpace(candidate.Explanation)) return null;

        return new GeneratedQuestion(candidate.Text.Trim(), candidate.Options, candidate.CorrectIndex, candidate.Explanation.Trim());
    }

    private static string BuildUserPrompt(CefrLevel level, SkillArea skill, IReadOnlyCollection<string> avoidTexts)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Level: {level}");
        sb.AppendLine($"Skill: {skill}");

        if (avoidTexts.Count > 0)
        {
            sb.AppendLine("Write one new question different from all of these already in the bank:");
            foreach (var text in avoidTexts) sb.AppendLine($"- {text}");
        }

        return sb.ToString();
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

    private record GeneratedQuestionJson(string? Text, List<string>? Options, int CorrectIndex, string? Explanation);
}
