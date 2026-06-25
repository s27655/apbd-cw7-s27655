using Microsoft.AspNetCore.Mvc;
using MiniHelpdesk.Web.Interfaces;
using MiniHelpdesk.Web.ViewModels;

namespace MiniHelpdesk.Web.Controllers;

public class TicketsController : Controller
{
    private readonly ITicketService _service;

    public TicketsController(ITicketService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var tickets = await _service.GetAllTicketsAsync();
        return View(tickets);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateTicketViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTicketViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _service.CreateTicketAsync(model);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var viewModel = await _service.GetTicketDetailsAsync(id);
        if (viewModel is null)
            return NotFound();

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        await _service.CloseTicketAsync(id);
        return RedirectToAction(nameof(Details), new { id });
    }
}
