using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Comjustinspicer.CMS;
using Serilog;
using Serilog.Events; // may still be used by other code; kept for now
using Comjustinspicer.CMS.Logging;
using Comjustinspicer.CMS.Data.DbContexts;
using Comjustinspicer;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

MapTypes(builder.Services);

builder.Services.AddComjustinspicerCms(builder.Configuration);

builder.Host.UseCmsSerilog(builder.Configuration);

// Add MVC and enable runtime compilation for Razor views in Development so changes to .cshtml are picked up
var mvc = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
{
    mvc.AddRazorRuntimeCompilation();
}

try
{
    var app = builder.Build();

    app.EnsureCMS();

    if (!app.Environment.IsDevelopment())
    {
        // Route exceptions to centralized ErrorController
        app.UseExceptionHandler("/Error");
        // Route status code pages (like 404) to the ErrorController status handler
        app.UseStatusCodePagesWithReExecute("/Error/{0}");
    }

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (HostAbortedException)
{
    // Expected when running EF Core tools (migrations, etc.) - not an error
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}


static void MapTypes(IServiceCollection services)
{
    services.AddAutoMapper(cfg =>
    {
        cfg.AddProfile(new MappingProfile());
    });

}
