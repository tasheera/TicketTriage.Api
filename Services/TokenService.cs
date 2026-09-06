using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TicketTriage.Api;

public class TokenService
{
    private readonly IConfiguration _configuration;
    public TokenService (IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Agent agent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, agent.Id.ToString()),
            new(ClaimTypes.Email, agent.Email),
            new(ClaimTypes.Role, agent.Role.ToString())
        };

        var key= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_SECRET"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken (
            issuer: _configuration["JWT_ISSUER"],
            audience: _configuration["JWT_AUDIENCE"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
