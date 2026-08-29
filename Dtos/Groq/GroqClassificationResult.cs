using System.Text.Json.Serialization;

namespace TicketTriage.Api;

public record GroqClassificationResult
(
    [property: JsonPropertyName("category")]string Category,
    [property: JsonPropertyName("priority")]string Priority,
    [property: JsonPropertyName("sentiment")]string Sentiment,
    [property: JsonPropertyName("reasoning")]string Reasoning

);