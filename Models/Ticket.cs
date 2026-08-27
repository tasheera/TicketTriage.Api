using System;

namespace TicketTriage.Api;

public class Ticket
{
    public int Id {get; set;}
    public required string CustomerName {get; set;}
    public required string CustomerEmail { get; set; }
    public required string Subject {get; set;}
    public required string Description {get; set;}
    
    public string? Category {get; set;}
    public string? Priority {get; set;}
    public string? Sentiment {get; set;}
    public string? AiReasoning {get; set;}
    
    public TicketStatus Status {get; set;} = TicketStatus.Open;
    public DateTime CreatedAt {get; set;} =  DateTime.UtcNow;
}

public enum TicketStatus
{
    Open,
    InProgress,
    Resolved
}
