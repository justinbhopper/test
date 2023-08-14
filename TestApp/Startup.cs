using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RH.Polaris.AppInit;

namespace TestApp;

public class Startup : IConfigureApplication
{
    public void Configure(IApplicationBuilder app)
    {
        app.UseMiddleware<LoggingMiddleware>();
        app.UseEndpoints(e => e.MapControllers());
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();
    }
}
