using System;

namespace TicketTriage.Api;

public class Agent
{
    public int Id {get; set;}
    public required string Name {get; set;}
    public required string Email {get; set;}
    public required string PasswordHash {get; set;}
    public AgentRole Role { get; set; } = AgentRole.Agent;


}

public enum AgentRole
{
    Agent,
    Admin
}