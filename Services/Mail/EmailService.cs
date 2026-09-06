using System;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

using Microsoft.Extensions.Options;

namespace TicketTriage.Api;

public class EmailService : IEmailService
{

    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendConfirmationAsync(Ticket ticket)
    {
        {
            var subject = $"Ticket #{ticket.Id} Received ✅";

            var body = $"""
        <h2>Hi {ticket.CustomerName}, we've received your support ticket!</h2>
        <p>Your ticket has been logged and triaged automatically by our AI system.</p>
        <hr/>
        <table style="border-collapse: collapse; width: 100%;">
            <tr><td style="padding: 8px; font-weight: bold;">Ticket ID</td><td style="padding: 8px;">#{ticket.Id}</td></tr>
            <tr><td style="padding: 8px; font-weight: bold;">Subject</td><td style="padding: 8px;">{ticket.Subject}</td></tr>
            <tr><td style="padding: 8px; font-weight: bold;">Category</td><td style="padding: 8px;">{ticket.Category ?? "Pending"}</td></tr>
            <tr><td style="padding: 8px; font-weight: bold;">Submitted At</td><td style="padding: 8px;">{ticket.CreatedAt:MMM dd, yyyy hh:mm tt} (UTC)</td></tr>
        </table>
        <hr/>
        <p><strong>Description:</strong></p>
        <p>{ticket.Description}</p>
        <br/>
        <p>Our team will review your ticket and get back to you shortly.</p>
    """;

            await SendEmailAsync(ticket.CustomerEmail, subject, body);
        }
    }


    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var email = new MimeMessage();

        //from
        email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));

        //to
        email.To.Add(MailboxAddress.Parse(toEmail));

        email.Subject = subject;

        //build email body
        email.Body = new TextPart("html") { Text = htmlBody };

        //send email
        using var smtp = new SmtpClient();

        // Bypass SSL certificate validation
        smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
