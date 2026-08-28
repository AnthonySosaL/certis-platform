namespace EnglishC1.Client.Application.Speaking;

// Level is a free-text hint ("B1+"), not a CefrLevel - scenarios span a
// range rather than a single fixed level the way placement/reinforcement
// questions do.
public record SpeakingScenarioDto(string Id, string Title, string Level, string Description);

// "user" or "assistant" - mirrors the Groq/OpenAI chat message shape
// directly so SpeakingService can pass it straight through.
public record SpeakingTurnDto(string Role, string Content);

public record SpeakingReplyRequest(List<SpeakingTurnDto> History, string Message);

public record SpeakingReplyDto(string Reply);
