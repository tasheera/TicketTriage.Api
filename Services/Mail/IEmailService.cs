using System;

namespace TicketTriage.Api;

public interface IEmailService
{
    Task SendConfirmationAsync(Ticket ticket);
}
