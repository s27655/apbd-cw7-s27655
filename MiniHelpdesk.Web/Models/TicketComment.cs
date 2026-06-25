namespace MiniHelpdesk.Web.Models;

public class TicketComment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Ticket? Ticket { get; set; }
}
