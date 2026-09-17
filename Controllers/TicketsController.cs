using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace TicketTriage.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly GroqService _groq;
        private readonly IEmailService _emailService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(AppDbContext context, GroqService groq, IEmailService emailService, ILogger<TicketsController> logger)
        {
            _context = context;
            _groq = groq;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTickets(
            [FromQuery] string? status,
            [FromQuery] string? category,
            [FromQuery] string? priority
        )

        {
            IQueryable<Ticket> tickets = _context.Tickets;

            //status filter
            if (!string.IsNullOrEmpty(status))
            {
                if (!Enum.TryParse<TicketStatus>(status, ignoreCase: true, out var statusEnum))
                {
                    return Problem(
                        statusCode: 400,
                        title: "Invalid status value",
                        detail: $"Invalid status. Valid values : {string.Join(", ", Enum.GetNames<TicketStatus>())}"
                    );
                }

                tickets = tickets.Where(t => t.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(category))
            {
                tickets = tickets.Where(t => t.Category == category);
            }

            if (!string.IsNullOrEmpty(priority))
            {
                tickets = tickets.Where(t => t.Priority == priority);

            }


            //sorting
            var ticketList = await tickets
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(ticketList.Select(t => t.ToResponse()).ToList());

        }


        [AllowAnonymous]
        [EnableRateLimiting("TicketSubmission")]
        [HttpPost]
        public async Task<IActionResult> CretaeTicket(CreateTicketRequest request)
        {
            var ticket = request.ToEntity();


            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var result = await _groq.ClassifyTicketAsync(
                request.Subject,
                request.Description
            );

            if (result is not null)
            {

                ticket.Category = result.Category;
                ticket.Priority = result.Priority;
                ticket.Sentiment = result.Sentiment;
                ticket.AiReasoning = result.Reasoning;
            }
            else
            {
                ticket.Category = "Unclassified";
                ticket.Priority = "Medium";
                ticket.Sentiment = "Unclassified";
                ticket.AiReasoning = "Automatic classification unavailable - please review manually.";
            }

            await _context.SaveChangesAsync();

            _ = Task.Run(async () => // fire and forget
            {
                try
                {
                    await _emailService.SendConfirmationAsync(ticket);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send confirmation email for ticket #{TicketId}", ticket.Id);
                }
            });

            return CreatedAtAction(
                nameof(GetTicket),
                new { id = ticket.Id },
                ticket.ToResponse()
            );
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket is null)
            {
                return Problem(statusCode: 404, title: "Ticket not found", detail: $"No ticket found with id {id}");
            }

            return Ok(ticket.ToResponse());
        }




        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTicketStatus(int id, UpdateTicketStatusRequest request)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket is null)
            {
                return Problem(statusCode: 404, title: "Ticket not found", detail: $"No ticket found with id {id}");
            }

            if (!Enum.TryParse<TicketStatus>(request.Status, ignoreCase: true, out var newStatus))
            {
                return Problem(
                    statusCode: 400,
                    title: "Invalid status value",
                    detail: $"Invalid status. Valid values : {string.Join(", ", Enum.GetNames<TicketStatus>())}"
                );
            }

            ticket.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(ticket.ToResponse());


        }

    }
}
