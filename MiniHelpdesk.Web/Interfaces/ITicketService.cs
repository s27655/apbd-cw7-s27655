using MiniHelpdesk.Web.Models;
using MiniHelpdesk.Web.ViewModels;

namespace MiniHelpdesk.Web.Interfaces;

public interface ITicketService
{
    Task<IEnumerable<Ticket>> GetAllTicketsAsync();
    Task<TicketDetailsViewModel?> GetTicketDetailsAsync(int id);
    Task CreateTicketAsync(CreateTicketViewModel model);
    Task CloseTicketAsync(int id);
}
