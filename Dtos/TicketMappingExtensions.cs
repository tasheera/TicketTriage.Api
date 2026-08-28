using System;

namespace TicketTriage.Api;

public static class TicketMappingExtensions
{
    public static Ticket ToEntity(this CreateTicketRequest createTicketRequest)
    {
        return new Ticket
        {
            CustomerName = createTicketRequest.CustomerName,
            CustomerEmail = createTicketRequest.CustomerEmail,
            Subject = createTicketRequest.Subject,
            Description = createTicketRequest.Description
        };


    }


    public static TicketResponse ToResponse(this Ticket ticket)
    {
        return new TicketResponse
        (
            ticket.Id,
            ticket.CustomerName,
            ticket.CustomerEmail,
            ticket.Subject,
            ticket.Description,
            ticket.Category,
            ticket.Priority,
            ticket.Sentiment,
            ticket.AiReasoning,
            ticket.Status.ToString(),
            ticket.CreatedAt
        );
    }
}
