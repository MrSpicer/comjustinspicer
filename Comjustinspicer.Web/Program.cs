using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using comjustinspicer.Data;
using Serilog;
using Serilog.Events;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog early so startup logs are captured
var cfgConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "DataSource=app.db";
// extract file path from DataSource=...;Cache=... style connection string if possible
string ExtractSqliteFile(string conn)
{
    var m = Regex.Match(conn, "DataSource=(?<file>[^;]+)", RegexOptions.IgnoreCase);
    return m.Success ? m.Groups["file"].Value : conn;
}

var sqliteFile = ExtractSqliteFile(cfgConn);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    // Fallback to a rolling file sink. If you need DB storage, see the EF-background-writer
    // approach (or use a compatible Serilog sink for your DB provider).
    .WriteTo.File("Logs/log-.txt", rollingInterval: Serilog.RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory_Application")));

// Register BlogContext using same connection (separate DB file supported via configuration if desired)
builder.Services.AddDbContext<BlogContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsHistoryTable("__EFMigrationsHistory_Blog")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Register blog post service
builder.Services.AddScoped<comjustinspicer.Data.Models.Blog.IPostService, comjustinspicer.Data.Models.Blog.PostService>();
    
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI();

// Development email sender - logs confirmation emails to Serilog and a local file.
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, comjustinspicer.Services.DevEmailSender>();
//

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable authentication middleware so Identity can sign users in/out.
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Apply any pending EF Core migrations so the database schemas are up-to-date.
// This will ensure both the Identity/ApplicationDbContext schema and the BlogContext schema
// are created when the app starts (useful for simple deployments/dev). If you prefer to run
// migrations manually, remove or comment out this block.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var appDb = services.GetRequiredService<ApplicationDbContext>();
        // Apply Identity / application schema migrations
        appDb.Database.Migrate();

        var blogContext = services.GetRequiredService<BlogContext>();
        // Apply migrations for the blog schema
        blogContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = Serilog.Log.ForContext<Program>();
        logger.Error(ex, "An error occurred migrating the databases.");
        throw;
    }
}

app.Run();
