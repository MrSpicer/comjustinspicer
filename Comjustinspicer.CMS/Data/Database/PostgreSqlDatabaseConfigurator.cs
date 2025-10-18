using Comjustinspicer.CMS.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Comjustinspicer.CMS.Data.Database;

public sealed class PostgreSqlDatabaseConfigurator : IDatabaseConfigurator
{
    public string DBTypeSupported => "PostgreSQL";

    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory_Application")));

        services.AddDbContext<BlogContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory_Blog")));

        services.AddDbContext<ContentBlockContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory_ContentBlock")));
    }
}
