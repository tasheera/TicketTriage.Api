using System;
using System.ComponentModel.DataAnnotations;

namespace TicketTriage.Api;

public record LoginRequest
(
    [Required, EmailAddress] string Email,
    [Required] string Password
);
