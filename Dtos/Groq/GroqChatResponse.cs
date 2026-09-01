using System.Text.Json.Serialization;

namespace TicketTriage.Api;

public record GroqChatResponse
(
    [property : JsonPropertyName("choices")] List<GroqChoice> Choices
);

public record GroqChoice (
    [property: JsonPropertyName("message")] GroqResponseMessage Message
);

public record GroqResponseMessage (
    [property: JsonPropertyName("content")] string Content
);