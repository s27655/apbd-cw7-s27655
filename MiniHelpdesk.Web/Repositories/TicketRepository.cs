using Microsoft.EntityFrameworkCore;
using MiniHelpdesk.Web.Data;
using MiniHelpdesk.Web.Interfaces;
using MiniHelpdesk.Web.Models;

namespace MiniHelpdesk.Web.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _db;

    public TicketRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Ticket>> GetAllAsync()
    {
        return await _db.Tickets
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Ticket?> GetByIdWithCommentsAsync(int id)
    {
        return await _db.Tickets
            .Include(t => t.Comments.OrderBy(c => c.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task CreateTicketWithCommentAsync(Ticket ticket, TicketComment comment)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            comment.TicketId = ticket.Id;
            _db.TicketComments.Add(comment);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CloseTicketAsync(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket is null) return;

        ticket.Status = TicketStatus.Closed;
        await _db.SaveChangesAsync();
    }
}
