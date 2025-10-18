using Comjustinspicer.CMS.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Comjustinspicer.CMS.Data;

namespace Comjustinspicer.CMS.Data.Database;

public sealed class SqliteDatabaseConfigurator : IDatabaseConfigurator
{
    public string DBTypeSupported => "SQLite";

    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory_Application")));

        services.AddDbContext<BlogContext>(options =>
            options.UseSqlite(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory_Blog")));

        services.AddDbContext<ContentBlockContext>(options =>
            options.UseSqlite(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory_ContentBlock")));
    }
}
