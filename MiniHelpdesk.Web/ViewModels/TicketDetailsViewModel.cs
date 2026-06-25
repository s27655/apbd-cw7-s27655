using MiniHelpdesk.Web.Models;

namespace MiniHelpdesk.Web.ViewModels;

public class TicketDetailsViewModel
{
    public Ticket Ticket { get; set; } = null!;
    public IEnumerable<TicketComment> Comments { get; set; } = Enumerable.Empty<TicketComment>();
}
