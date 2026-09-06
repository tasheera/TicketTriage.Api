using Microsoft.EntityFrameworkCore;

namespace TicketTriage.Api;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Agent> Agents => Set<Agent>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Ticket>()
        .Property(t => t.Status)
        .HasConversion<string>();

        modelBuilder.Entity<Agent>()
        .Property(a => a.Role)
        .HasConversion<string>();

        modelBuilder.Entity<Agent>()
        .HasIndex(a => a.Email)
        .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}