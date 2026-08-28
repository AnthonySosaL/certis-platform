using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnglishC1.Client.Application.Speaking;
using EnglishC1.Client.Infrastructure.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishC1.Client.Infrastructure.Speaking;

// Stateless on purpose for this first slice - the full conversation
// history is sent by the client on every turn instead of being persisted
// server-side. No new table, no attempt/session concept to design yet;
// revisit if this needs to survive a reload or show up in the student's
// history later.
public class SpeakingService(HttpClient http, IOptions<GroqOptions> options, ILogger<SpeakingService> logger)
    : ISpeakingService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
    private const string Model = "openai/gpt-oss-20b";

    public List<SpeakingScenarioDto> GetScenarios() =>
        SpeakingScenarios.All.Select(s => new SpeakingScenarioDto(s.Id, s.Title, s.Level, s.Description)).ToList();

    public async Task<string?> GenerateReplyAsync(string scenarioId, List<SpeakingTurnDto> history, string message, CancellationToken ct = default)
    {
        var scenario = SpeakingScenarios.All.FirstOrDefault(s => s.Id == scenarioId);
        if (scenario is null) return null;

        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey)) return null;

        try
        {
            var messages = new List<GroqChatMessage> { new("system", scenario.SystemPrompt) };
            // Cap history sent to Groq - a long practice session shouldn't
            // grow the prompt (and cost) without bound; the last dozen
            // turns is plenty of context for a natural reply.
            messages.AddRange(history.TakeLast(12).Select(t => new GroqChatMessage(t.Role, t.Content)));
            messages.Add(new GroqChatMessage("user", message));

            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = JsonContent.Create(
                new GroqChatRequest(Model, messages, Temperature: 0.8, MaxTokens: 300, ReasoningEffort: "low"),
                options: JsonOptions);

            using var response = await http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Groq speaking request failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<GroqChatResponse>(JsonOptions, ct);
            var reply = payload?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            return string.IsNullOrWhiteSpace(reply) ? null : reply;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Groq speaking request threw");
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
