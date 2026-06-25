using MiniHelpdesk.Web.Interfaces;
using MiniHelpdesk.Web.Models;

namespace MiniHelpdesk.Tests.Fakes;

public class FakeTicketRepository : ITicketRepository
{
    private readonly List<Ticket> _tickets = new();
    private readonly List<TicketComment> _comments = new();
    private int _nextTicketId = 1;
    private int _nextCommentId = 1;

    public bool ThrowOnSaveComment { get; set; }

    public Task<IEnumerable<Ticket>> GetAllAsync()
        => Task.FromResult<IEnumerable<Ticket>>(_tickets.OrderByDescending(t => t.CreatedAt).ToList());

    public Task<Ticket?> GetByIdWithCommentsAsync(int id)
    {
        var ticket = _tickets.FirstOrDefault(t => t.Id == id);
        if (ticket != null)
            ticket.Comments = _comments.Where(c => c.TicketId == id).OrderBy(c => c.CreatedAt).ToList();
        return Task.FromResult(ticket);
    }

    public Task CreateTicketWithCommentAsync(Ticket ticket, TicketComment comment)
    {
        ticket.Id = _nextTicketId++;
        _tickets.Add(ticket);

        if (ThrowOnSaveComment)
            throw new InvalidOperationException("Symulowany błąd zapisu komentarza.");

        comment.Id = _nextCommentId++;
        comment.TicketId = ticket.Id;
        _comments.Add(comment);

        return Task.CompletedTask;
    }

    public Task CloseTicketAsync(int id)
    {
        var ticket = _tickets.FirstOrDefault(t => t.Id == id);
        if (ticket != null)
            ticket.Status = TicketStatus.Closed;
        return Task.CompletedTask;
    }

    public IReadOnlyList<Ticket> AllTickets => _tickets.AsReadOnly();
    public IReadOnlyList<TicketComment> AllComments => _comments.AsReadOnly();
}
