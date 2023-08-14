using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TestApp;

[Route(@"/ShouldLog/[action]")]
[LogUnhandledExceptions]
public class ShouldLogController : Controller
{
    [HttpGet]
    [ActionName(@"test")]
    public IActionResult Test()
    {
        return View(42);
    }
}

[Route(@"/DontLog/[action]")]
public class DontLogController : Controller
{
    [HttpGet]
    [ActionName(@"test")]
    public IActionResult Test()
    {
        return View(42);
    }
}

public class LogUnhandledExceptionsAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Items.Add("log-unhandled-exceptions", true);

        base.OnActionExecuting(context);
    }
}

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (context.Items.ContainsKey("log-unhandled-exceptions"))
        {
            Console.WriteLine("Handled exception!" + ex.ToString());
        }
    }
}
