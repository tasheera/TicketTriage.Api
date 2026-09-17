using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TicketTriage.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, TokenService tokenService, ILogger<AuthController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Email == loginRequest.Email);

            if(agent is null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, agent.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", loginRequest.Email);
                return Problem(
                    statusCode:401,
                    title: "Invalid credentials",
                    detail: "Email or password is incorrect"
                );
            }

            var token = _tokenService.GenerateToken(agent);

            _logger.LogInformation("Agent {Email} logged in successfully", agent.Email);

            return Ok(new LoginResponse(
                Token: token,
                Name:agent.Name,
                Email: agent.Email,
                Role: agent.Role.ToString()
            ));
        }

    }
}
