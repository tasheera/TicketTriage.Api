using System;

namespace TicketTriage.Api;

public record LoginResponse(
    string Token,
    string Name,
    string Email,
    string Role
);
