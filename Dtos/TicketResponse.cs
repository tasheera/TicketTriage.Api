namespace TicketTriage.Api;

public record TicketResponse (
    
    int Id,
    string CustomerName,
    string CustomerEmail,
    string Subject,
    string Description,
    string? Category,
    string? Priority,
    string? Sentiment,
    string? AiReasoning,
    string Status,
    DateTime CreatedAt
);