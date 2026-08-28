using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TicketTriage.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CretaeTicket(CreateTicketRequest request)
        {
            var ticket = request.ToEntity();

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

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
                return Problem (statusCode:404, title: "Ticket not found", detail: $"No ticket found with id {id}" );
            }

            return Ok(ticket.ToResponse());
        }


    }
}
