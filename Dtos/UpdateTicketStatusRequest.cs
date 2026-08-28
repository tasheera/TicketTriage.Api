using System.ComponentModel.DataAnnotations;

namespace TicketTriage.Api;

public record UpdateTicketStatusRequest
(
    [Required] string Status
);