namespace MiniHelpdesk.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Nieobsłużony wyjątek podczas przetwarzania żądania {Path}", context.Request.Path);
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(
                "<h2>Wystąpił błąd serwera.</h2>" +
                "<p>Przepraszamy, coś poszło nie tak. Spróbuj ponownie później.</p>" +
                "<a href='/'>Wróć na stronę główną</a>");
        }
    }
}
