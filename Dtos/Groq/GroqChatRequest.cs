using System.Text.Json.Serialization;

namespace TicketTriage.Api;

public record GroqChatRequest
(
    [property: JsonPropertyName("model")] string Model, // property: JsonPropertyName used for C# naming conventions(snake_case vs PascalCase)
    [property: JsonPropertyName("messages")] List<GroqMessage> Messages,
    [property: JsonPropertyName("response_format")] GroqResponseFormat ResponseFormat,
    [property: JsonPropertyName("temperature")] double Temperature = 0
);


public record GroqMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content
);

public record GroqResponseFormat(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("json_schema")] GroqJsonSchema JsonSchema
);

public record GroqJsonSchema
(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("strict")] bool Strict,
    [property: JsonPropertyName("schema")] object Schema
);