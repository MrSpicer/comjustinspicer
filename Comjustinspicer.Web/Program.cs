using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Comjustinspicer.CMS;
using Serilog;
using Serilog.Events; // may still be used by other code; kept for now
using Comjustinspicer.CMS.Logging;
using Comjustinspicer.CMS.Data.DbContexts;
using Comjustinspicer.Models.Blog;
using Comjustinspicer;

var builder = WebApplication.CreateBuilder(args);

MapTypes(builder.Services);

builder.Services.AddComjustinspicerCms(builder.Configuration);

builder.Host.UseCmsSerilog(builder.Configuration);

MapControllers(builder.Services, builder.Configuration, builder.Environment);

try
{
    var app = builder.Build();

    app.EnsureCMS();

    ConfigureMiddleware(app);

    app.Run();
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

// --- Local helper implementations ---


static void MapControllers(IServiceCollection services, ConfigurationManager configuration, IWebHostEnvironment environment)
{
    // Add MVC and enable runtime compilation for Razor views in Development so changes to .cshtml are picked up
    var mvc = services.AddControllersWithViews();
    if (environment.IsDevelopment())
    {
        mvc.AddRazorRuntimeCompilation();
    }
}

static void MapTypes(IServiceCollection services)
{
    services.AddScoped<IBlogModel, BlogModel>();
    services.AddScoped<IBlogPostModel, BlogPostModel>();


    services.AddAutoMapper(cfg =>
    {
        cfg.AddProfile(new MappingProfile());
    });

}

static void ConfigureMiddleware(WebApplication app)
{
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
}
