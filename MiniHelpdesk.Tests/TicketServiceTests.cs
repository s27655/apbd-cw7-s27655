using MiniHelpdesk.Tests.Fakes;
using MiniHelpdesk.Web.Models;
using MiniHelpdesk.Web.Services;
using MiniHelpdesk.Web.ViewModels;

namespace MiniHelpdesk.Tests;

public class TicketServiceTests
{
    private static TicketService BuildService(FakeTicketRepository repo) => new(repo);

    // Test 1: Poprawne zgłoszenie zostaje utworzone
    [Fact]
    public async Task CreateTicketAsync_ValidModel_CreatesTicketAndComment()
    {
        var repo = new FakeTicketRepository();
        var service = BuildService(repo);

        var model = new CreateTicketViewModel
        {
            Title = "Brak dostępu do systemu",
            Description = "Nie mogę się zalogować od rana.",
            FirstCommentAuthor = "Jan Kowalski",
            FirstCommentContent = "Sprawdziłem hasło, na pewno poprawne."
        };

        await service.CreateTicketAsync(model);

        Assert.Single(repo.AllTickets);
        Assert.Single(repo.AllComments);
        Assert.Equal("Brak dostępu do systemu", repo.AllTickets[0].Title);
        Assert.Equal(TicketStatus.Open, repo.AllTickets[0].Status);
        Assert.Equal("Jan Kowalski", repo.AllComments[0].Author);
    }

    // Test 2: Puste Title powoduje wyjątek walidacyjny
    [Fact]
    public async Task CreateTicketAsync_EmptyTitle_ThrowsArgumentException()
    {
        var repo = new FakeTicketRepository();
        var service = BuildService(repo);

        var model = new CreateTicketViewModel
        {
            Title = "   ",
            FirstCommentAuthor = "Jan",
            FirstCommentContent = "Treść komentarza."
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateTicketAsync(model));

        Assert.Empty(repo.AllTickets);
        Assert.Empty(repo.AllComments);
    }

    // Test 3: Pusta treść komentarza powoduje wyjątek walidacyjny
    [Fact]
    public async Task CreateTicketAsync_EmptyCommentContent_ThrowsArgumentException()
    {
        var repo = new FakeTicketRepository();
        var service = BuildService(repo);

        var model = new CreateTicketViewModel
        {
            Title = "Problem z drukarką",
            FirstCommentAuthor = "Anna Nowak",
            FirstCommentContent = ""
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateTicketAsync(model));

        Assert.Empty(repo.AllTickets);
    }

    // Test 4: Zamknięcie zgłoszenia zmienia status na Closed
    [Fact]
    public async Task CloseTicketAsync_ExistingTicket_ChangesStatusToClosed()
    {
        var repo = new FakeTicketRepository();
        var service = BuildService(repo);

        await service.CreateTicketAsync(new CreateTicketViewModel
        {
            Title = "Slow PC",
            FirstCommentAuthor = "Admin",
            FirstCommentContent = "Komputer mocno muleje."
        });

        var ticketId = repo.AllTickets[0].Id;
        await service.CloseTicketAsync(ticketId);

        var ticket = await repo.GetByIdWithCommentsAsync(ticketId);
        Assert.Equal(TicketStatus.Closed, ticket!.Status);
    }

    // Test 5: Zamknięcie nieistniejącego zgłoszenia rzuca wyjątek
    [Fact]
    public async Task CloseTicketAsync_NonExistentTicket_ThrowsInvalidOperationException()
    {
        var repo = new FakeTicketRepository();
        var service = BuildService(repo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CloseTicketAsync(999));
    }

    // Test 6: GetTicketDetailsAsync dla nieistniejącego id zwraca null
    [Fact]
    public async Task GetTicketDetailsAsync_NonExistentId_ReturnsNull()
    {
        var repo = new FakeTicketRepository();
        var service = BuildService(repo);

        var result = await service.GetTicketDetailsAsync(42);

        Assert.Null(result);
    }
}
