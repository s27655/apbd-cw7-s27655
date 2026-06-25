using Microsoft.EntityFrameworkCore;
using MiniHelpdesk.Web.Data;
using MiniHelpdesk.Web.Interfaces;
using MiniHelpdesk.Web.Middleware;
using MiniHelpdesk.Web.Repositories;
using MiniHelpdesk.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=helpdesk.db"));

builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketService, TicketService>();

var app = builder.Build();

// Własne middleware muszą być zarejestrowane przed UseRouting,
// aby obejmowały całe żądanie łącznie z routingiem i kontrolerami.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "tickets",
    pattern: "Tickets/{action=Index}/{id?}",
    defaults: new { controller = "Tickets" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tickets}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

public partial class Program { }
