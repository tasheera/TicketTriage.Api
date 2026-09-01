using System;
using System.Net.Http.Json;
using System.Text.Json;

namespace TicketTriage.Api;

public class GroqService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<GroqService> _logger;

    private static readonly object ClassificationSchema = new
    {
        type = "object",
        properties = new
        {
            category = new { type = "string", @enum = new[] { "Technical", "Billing", "Account", "General" } },
            priority = new { type = "string", @enum = new[] { "Urgent", "High", "Medium", "Low" } },
            sentiment = new { type = "string", @enum = new[] { "Frustrated", "Neutral", "Positive" } },
            reasoning = new { type = "string" }
        },
        required = new[] { "category", "priority", "sentiment", "reasoning" },
        additionalProperties = false
    };

    private const string SystemPrompt = """
    You are a support ticket triage assistant. Classify each incoming ticket based on its subject and description.

    Guidelines:
    - category: Technical = bugs, errors, crashes. Billing = payments, invoices, refunds. Account = login, password, access issues. General = anything else, including feedback and questions.
    - priority: Urgent = customer explicitly states they are blocked right now, mentions a deadline, or uses emergency language ("right now", "immediately", "today"). Do NOT classify as Urgent based on emotional intensity alone — strong or frustrated language without an explicit time constraint or blocking impact is High, not Urgent. High = significant impact but not blocking, no explicit deadline. Medium = normal issue, no urgency signals either way. Low = no urgency signals present, OR the customer explicitly states they are not in a hurry ("no rush", "not urgent", "someday", "whenever").
    - sentiment: Frustrated = customer expresses annoyance, anger, or repeated failed attempts. Neutral = matter-of-fact tone. Positive = compliment or calm feature request. Judge tone only, independent of priority — a calmly-written ticket about an urgent issue is still Neutral, and an angry ticket about a minor issue is still Frustrated. Do not let urgency influence the sentiment label or vice versa.
    - reasoning: one short sentence (under 20 words) explaining your classification.
    """;

    public GroqService(HttpClient httpClient, IConfiguration configuration, ILogger<GroqService> logger)
    {
        _httpClient = httpClient;
        _model = configuration["Groq:Model"] ?? "openai/gpt-oss-20b";
        _logger = logger;
    }

    public async Task<GroqClassificationResult> ClassifyTicketAsync(string subject, string description)
    {
        var request = new GroqChatRequest(
            Model: _model,
            Messages: new List<GroqMessage>
            {
                new("system", SystemPrompt),
                new("user", $"Subject: {subject}\nDescription: {description}")
            },
            ResponseFormat: new GroqResponseFormat(
                Type: "json_schema",
                JsonSchema: new GroqJsonSchema(
                    Name: "ticket_classification",
                    Strict: true,
                    Schema: ClassificationSchema
                )
            )
        );

        try
        {

            var httpResponse = await _httpClient.PostAsJsonAsync("chat/completions", request);
            httpResponse.EnsureSuccessStatusCode();

            var groqResponse = await httpResponse.Content.ReadFromJsonAsync<GroqChatResponse>();
            var content = groqResponse!.Choices[0].Message.Content;

            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("Groq returned an empty response");
                return null;
            }

            return JsonSerializer.Deserialize<GroqClassificationResult>(content)!;

        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Groq classification failed: HTTP error (network issue, rate limit, or server error)");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Groq classification failed: request timed out");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Groq classification failed: could not parse response");
            return null;
        }

    }
}