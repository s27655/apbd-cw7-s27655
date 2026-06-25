using MiniHelpdesk.Web.Interfaces;
using MiniHelpdesk.Web.Models;
using MiniHelpdesk.Web.ViewModels;

namespace MiniHelpdesk.Web.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _repository;

    public TicketService(ITicketRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        => _repository.GetAllAsync();

    public async Task<TicketDetailsViewModel?> GetTicketDetailsAsync(int id)
    {
        var ticket = await _repository.GetByIdWithCommentsAsync(id);
        if (ticket is null) return null;

        return new TicketDetailsViewModel
        {
            Ticket = ticket,
            Comments = ticket.Comments
        };
    }

    public async Task CreateTicketAsync(CreateTicketViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Title))
            throw new ArgumentException("Tytuł nie może być pusty.");

        if (string.IsNullOrWhiteSpace(model.FirstCommentContent))
            throw new ArgumentException("Treść pierwszego komentarza nie może być pusta.");

        var ticket = new Ticket
        {
            Title = model.Title.Trim(),
            Description = model.Description?.Trim() ?? string.Empty,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        var comment = new TicketComment
        {
            Author = model.FirstCommentAuthor?.Trim() ?? "Anonim",
            Content = model.FirstCommentContent.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateTicketWithCommentAsync(ticket, comment);
    }

    public async Task CloseTicketAsync(int id)
    {
        var details = await _repository.GetByIdWithCommentsAsync(id);
        if (details is null)
            throw new InvalidOperationException($"Zgłoszenie o id {id} nie istnieje.");

        await _repository.CloseTicketAsync(id);
    }
}
