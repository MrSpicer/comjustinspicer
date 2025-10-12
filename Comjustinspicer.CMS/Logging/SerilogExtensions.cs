using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;

namespace Comjustinspicer.CMS.Logging;

public static class SerilogExtensions
{
    public static IHostBuilder UseCmsSerilog(this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        // Local function replicating original extraction logic
        static string ExtractSqliteFile(string conn)
        {
            var m = Regex.Match(conn, "DataSource=(?<file>[^;]+)", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups["file"].Value : conn;
        }

        var conn = configuration.GetConnectionString("DefaultConnection") ?? "DataSource=app.db";
        var sqliteFile = ExtractSqliteFile(conn);

        if (sqliteFile.StartsWith("/data/", StringComparison.OrdinalIgnoreCase))
        {
            var dir = Path.GetDirectoryName(sqliteFile);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        hostBuilder.UseSerilog((context, services, loggerConfig) =>
		{
			// Start from configuration (allows overriding in appsettings.json / env vars)
			loggerConfig
				.ReadFrom.Configuration(context.Configuration)
				.ReadFrom.Services(services)
				.Enrich.FromLogContext();

			// Provide reasonable defaults if not specified
			loggerConfig.MinimumLevel.Override("Microsoft", LogEventLevel.Information);

			// Always log to console
			loggerConfig.WriteTo.Console();

			// Preserve local rolling file sink for developers
			if (!runningInContainer)
			{
				loggerConfig.WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day);
			}

			// Todo: Serilog.Sinks.SQLite is already referenced; configuration can enable it via appsettings
		});

        return hostBuilder;
    }
}
