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

// Ensure roles and an initial admin user exist
SeedRolesAndAdmin(app);

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

    services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddRoles<IdentityRole>()
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

static void SeedRolesAndAdmin(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = Serilog.Log.ForContext<Program>();

    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var config = services.GetRequiredService<IConfiguration>();

        var roles = new[] { "Admin", "Editor", "User" };
        foreach (var role in roles)
        {
            var exists = roleManager.RoleExistsAsync(role).GetAwaiter().GetResult();
            if (!exists)
            {
                var r = roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                if (!r.Succeeded)
                {
                    logger.Warning("Failed to create role {Role}: {Errors}", role, string.Join(", ", r.Errors.Select(e => e.Description)));
                }
            }
        }

        // Admin credentials can be configured in appsettings (AdminUser:Email, AdminUser:Password)
        var adminEmail = config["AdminUser:Email"];
        var adminPassword = config["AdminUser:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.Warning("Admin user not created - missing AdminUser:Email or AdminUser:Password configuration.");
            return;
        }

        var admin = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (admin == null)
        {
            admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var cr = userManager.CreateAsync(admin, adminPassword).GetAwaiter().GetResult();
            if (!cr.Succeeded)
            {
                logger.Warning("Failed to create admin user {Email}: {Errors}", adminEmail, string.Join(", ", cr.Errors.Select(e => e.Description)));
            }
        }

        var inRole = userManager.IsInRoleAsync(admin, "Admin").GetAwaiter().GetResult();
        if (!inRole)
        {
            var ar = userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
            if (!ar.Succeeded)
            {
                logger.Warning("Failed to add admin user {Email} to Admin role: {Errors}", adminEmail, string.Join(", ", ar.Errors.Select(e => e.Description)));
            }
        }
    }
    catch (Exception ex)
    {
        Serilog.Log.ForContext<Program>().Error(ex, "An error occurred seeding roles/admin user.");
        // do not rethrow startup seeding errors to avoid masking migration exceptions
    }
}
