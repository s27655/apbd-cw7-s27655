using Microsoft.EntityFrameworkCore;
using MiniHelpdesk.Web.Models;

namespace MiniHelpdesk.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Title).IsRequired().HasMaxLength(200);
            e.Property(t => t.Description).HasMaxLength(2000);
            e.Property(t => t.Status).HasConversion<string>();
            e.HasMany(t => t.Comments)
             .WithOne(c => c.Ticket)
             .HasForeignKey(c => c.TicketId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TicketComment>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Author).IsRequired().HasMaxLength(100);
            e.Property(c => c.Content).IsRequired().HasMaxLength(2000);
        });
    }
}
