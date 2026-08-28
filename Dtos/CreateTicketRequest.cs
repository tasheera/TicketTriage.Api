using System.ComponentModel.DataAnnotations;

namespace TicketTriage.Api;

public record CreateTicketRequest
(
    [Required, MaxLength(100)] string CustomerName,
    [Required, EmailAddress] string CustomerEmail,
    [Required, MaxLength(200)] string Subject,
    [Required, MaxLength(5000)] string Description

);
