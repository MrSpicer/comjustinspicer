using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using comjustinspicer.Data;
using Serilog;
using Serilog.Events;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Initialize logging early so startup messages are captured
ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

// Register framework services
ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure HTTP pipeline
ConfigureMiddleware(app);

// Ensure DB schemas are created/applied at startup (optional - can be removed for manual migration workflows)
ApplyPendingMigrations(app);

app.Run();

// --- Local helper implementations ---

static void ConfigureSerilog(ConfigurationManager configuration)
{
    var conn = configuration.GetConnectionString("DefaultConnection") ?? "DataSource=app.db";

    static string ExtractSqliteFile(string conn)
    {
        var m = Regex.Match(conn, "DataSource=(?<file>[^;]+)", RegexOptions.IgnoreCase);
        return m.Success ? m.Groups["file"].Value : conn;
    }

    var sqliteFile = ExtractSqliteFile(conn);

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        // Keep a rolling file sink for local troubleshooting
        .WriteTo.File("Logs/log-.txt", rollingInterval: Serilog.RollingInterval.Day)
        .CreateLogger();
}

static void ConfigureServices(IServiceCollection services, ConfigurationManager configuration, IWebHostEnvironment environment)
{
    // Add MVC and enable runtime compilation for Razor views in Development so changes to .cshtml are picked up
    var mvc = services.AddControllersWithViews();
    if (environment.IsDevelopment())
    {
        mvc.AddRazorRuntimeCompilation();
    }

    var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    // Main application DB (Identity + app data)
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory_Application")));

    // Blog DB/context can share the same connection or be configured separately in appsettings
    services.AddDbContext<BlogContext>(options =>
        options.UseSqlite(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory_Blog")));

    services.AddDatabaseDeveloperPageExceptionFilter();

    // Register application services
    services.AddScoped<comjustinspicer.Data.Models.Blog.IPostService, comjustinspicer.Data.Models.Blog.PostService>();
    // Register blog model which encapsulates business logic for the BlogController
    services.AddScoped<comjustinspicer.Models.Blog.IBlogModel, comjustinspicer.Models.Blog.BlogModel>();
    services.AddScoped<comjustinspicer.Models.Blog.IBlogPostModel, comjustinspicer.Models.Blog.BlogPostModel>();

    services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultUI();

    // Development email sender - logs confirmation emails to Serilog and a local file
#if DEBUG
    services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, comjustinspicer.Services.DevEmailSender>();
#endif

}

static void ConfigureMiddleware(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
}

static void ApplyPendingMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var appDb = services.GetRequiredService<ApplicationDbContext>();
        appDb.Database.Migrate();

        var blogContext = services.GetRequiredService<BlogContext>();
        blogContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = Serilog.Log.ForContext<Program>();
        logger.Error(ex, "An error occurred migrating the databases.");
        throw;
    }
}
