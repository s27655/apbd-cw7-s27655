using MiniHelpdesk.Web.Models;

namespace MiniHelpdesk.Web.Interfaces;

public interface ITicketRepository
{
    Task<IEnumerable<Ticket>> GetAllAsync();
    Task<Ticket?> GetByIdWithCommentsAsync(int id);
    Task CreateTicketWithCommentAsync(Ticket ticket, TicketComment comment);
    Task CloseTicketAsync(int id);
}
