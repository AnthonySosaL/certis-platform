using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnglishC1.Client.Application.Ai;
using EnglishC1.Client.Application.PlacementTest;
using EnglishC1.Client.Domain.PlacementTest;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishC1.Client.Infrastructure.Ai;

// Calls Groq's OpenAI-compatible chat completions endpoint to turn one
// test result into a short, personalized diagnostic - the "why", not
// just the score. On-demand only (the student clicks a button for it),
// never generated automatically for every attempt: it's a real network
// call with real latency and API cost, and most attempts don't need it
// explained beyond the numbers.
public class GroqInsightService(HttpClient http, IOptions<GroqOptions> options, ILogger<GroqInsightService> logger)
    : IAiInsightService
{
    // Groq's OpenAI-compatible API uses camelCase-free lowercase JSON keys
    // ("role", "content", "choices"...). Web defaults (camelCase) happen
    // to match single-word property names, and PropertyNameCaseInsensitive
    // covers the rest without hand-writing [JsonPropertyName] on every
    // property - caught the response side of this silently returning
    // null (case-sensitive System.Text.Json default) via a real end-to-end
    // test even though the request side worked by luck (Groq's parser is
    // lenient about casing).
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
    // Groq's model lineup churns - llama-3.3-70b-versatile (the obvious
    // choice at write time) had already been retired by the time this
    // was tested for real, returning a 404. Confirmed available via
    // GET /openai/v1/models before picking this one.
    private const string Model = "openai/gpt-oss-20b";

    private const string SystemPrompt =
        "You are an encouraging English C1 exam coach for Certis, an English placement and practice " +
        "platform. Given one student's test result - their skill breakdown and the specific questions " +
        "they missed, with the correct answer and a short explanation for each - write a short " +
        "personalized diagnostic. Identify the pattern behind their mistakes (don't just restate the " +
        "list), then give one or two concrete next steps. End with a brief, genuine encouraging note. " +
        "Plain prose, no markdown, no bullet points, no headers. 80-120 words. Address the student as \"you\".";

    public async Task<string?> GenerateInsightAsync(TestResultDto result, CancellationToken ct = default)
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
                        new GroqChatMessage("user", BuildPrompt(result)),
                    ],
                    Temperature: 0.6,
                    MaxTokens: 350,
                    ReasoningEffort: "low"),
                options: JsonOptions);

            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Groq insight request failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<GroqChatResponse>(JsonOptions, ct);
            var text = payload?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Groq insight request threw");
            return null;
        }
    }

    private const string OverallSystemPrompt =
        "You are an encouraging English C1 exam coach for Certis, an English placement and practice " +
        "platform. Given a student's FULL history across every placement and reinforcement attempt " +
        "they've taken - performance aggregated per (level, skill) combination across all of it, not one " +
        "single attempt - write a short overall progress diagnostic. Identify the 2-3 skill areas most " +
        "worth focusing on next, note any real improvement visible if the same cell was attempted more " +
        "than once, and end with one concrete suggestion for what to practice this week. Plain prose, no " +
        "markdown, no bullet points, no headers. 90-130 words. Address the student as \"you\".";

    // Aggregates history rather than sending every attempt verbatim -
    // bounds the prompt size to roughly one line per (level, skill)
    // combination (at most ~20) regardless of how many attempts a
    // student has racked up, instead of growing unbounded with history.
    public async Task<string?> GenerateOverallInsightAsync(List<TestResultDto> history, CancellationToken ct = default)
    {
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey) || history.Count == 0) return null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = JsonContent.Create(
                new GroqChatRequest(
                    Model,
                    [
                        new GroqChatMessage("system", OverallSystemPrompt),
                        new GroqChatMessage("user", BuildOverallPrompt(history)),
                    ],
                    Temperature: 0.6,
                    MaxTokens: 400,
                    ReasoningEffort: "low"),
                options: JsonOptions);

            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Groq overall insight request failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<GroqChatResponse>(JsonOptions, ct);
            var text = payload?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Groq overall insight request threw");
            return null;
        }
    }

    private static string BuildOverallPrompt(List<TestResultDto> history)
    {
        var sb = new StringBuilder();
        var placementCount = history.Count(h => h.Kind == AttemptKind.Placement);
        var reinforcementCount = history.Count(h => h.Kind == AttemptKind.Reinforcement);
        sb.AppendLine($"Total attempts: {history.Count} ({placementCount} placement, {reinforcementCount} reinforcement).");

        var latestPlacement = history
            .Where(h => h.Kind == AttemptKind.Placement)
            .OrderByDescending(h => h.CompletedAtUtc)
            .FirstOrDefault();
        if (latestPlacement is not null)
            sb.AppendLine($"Current placement level: {(latestPlacement.PlacementResult?.ToString() ?? "below A2")}.");

        sb.AppendLine();
        sb.AppendLine("Aggregate performance per (level, skill) across every attempt:");
        var aggregated = history
            .SelectMany(h => h.Breakdown)
            .GroupBy(b => (b.Level, b.SkillArea))
            .Select(g => new
            {
                g.Key.Level,
                g.Key.SkillArea,
                Correct = g.Sum(b => b.Correct),
                Total = g.Sum(b => b.Total),
                Attempts = g.Count(),
            })
            .OrderBy(x => x.Level)
            .ThenBy(x => x.SkillArea);

        foreach (var cell in aggregated)
        {
            var grade = cell.Total > 0 ? Math.Round((double)cell.Correct / cell.Total * 10, 1) : 0;
            sb.AppendLine($"- {cell.Level} {cell.SkillArea}: {cell.Correct}/{cell.Total} correct across {cell.Attempts} attempt(s), grade {grade}/10.");
        }

        return sb.ToString();
    }

    private static string BuildPrompt(TestResultDto result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Attempt type: {result.Kind}");
        sb.AppendLine($"Score: {result.Score}/{result.TotalQuestions}");
        sb.AppendLine($"Placement level: {(result.PlacementResult?.ToString() ?? "below A2")}");
        sb.AppendLine();
        sb.AppendLine("Skill breakdown:");
        foreach (var cell in result.Breakdown)
        {
            var status = cell.NeedsReinforcement ? "needs reinforcement" : "solid";
            sb.AppendLine($"- {cell.Level} {cell.SkillArea}: {cell.Correct}/{cell.Total} ({status})");
        }

        if (result.MissedQuestions.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Questions missed:");
            foreach (var q in result.MissedQuestions)
            {
                sb.AppendLine($"- \"{q.QuestionText}\" - answered \"{q.YourAnswerText}\", correct was \"{q.CorrectAnswerText}\".");
                if (!string.IsNullOrWhiteSpace(q.Explanation))
                    sb.AppendLine($"  Why: {q.Explanation}");
            }
        }

        return sb.ToString();
    }

    // ReasoningEffort matters specifically because Model is a reasoning
    // model (gpt-oss): without capping it, a real test run spent 218 of a
    // 220 max_tokens budget on hidden chain-of-thought and returned empty
    // content with finish_reason "length" - "low" keeps reasoning to a
    // handful of tokens so the budget goes to the actual answer.
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
