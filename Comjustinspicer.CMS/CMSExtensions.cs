using Comjustinspicer.CMS.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Comjustinspicer.CMS;

public static class CMSExtensions
{
	public static WebApplication EnsureCMS(this WebApplication app, bool throwOnError = true)
	{
		app.ApplyCmsPendingMigrations(throwOnError);
		app.EnsureCmsRolesAndAdminSeeded(throwOnError);
		app.ConfigureMiddleware(throwOnError);
		return app;
	}
	/// <summary>
	/// Applies any pending EF Core migrations for the CMS related contexts. Safe to call multiple times.
	/// Controlled by optional environment variable COMJUSTINSPICER_APPLY_MIGRATIONS (default true) or ASPNETCORE_ENVIRONMENT.
	/// </summary>
	/// <param name="app">The WebApplication.</param>
	/// <param name="throwOnError">If true, rethrows the exception after logging. Defaults to true (startup should fail if migrations fail).</param>
	/// <returns>The same <see cref="WebApplication"/> instance for chaining.</returns>
	private static WebApplication ApplyCmsPendingMigrations(this WebApplication app, bool throwOnError = true)
	{
		// Allow skipping via env var (e.g. for read-only replicas or integration tests)
		var skip = Environment.GetEnvironmentVariable("COMJUSTINSPICER_SKIP_MIGRATIONS");
		if (string.Equals(skip, "true", StringComparison.OrdinalIgnoreCase))
		{
			Log.ForContext(typeof(CMSExtensions)).Information("Skipping CMS migrations due to COMJUSTINSPICER_SKIP_MIGRATIONS=true");
			return app;
		}

		using var scope = app.Services.CreateScope();
		var services = scope.ServiceProvider;
		var logger = Log.ForContext(typeof(CMSExtensions));

		try
		{
			// Order shouldn't matter for independent contexts, but we keep a deterministic order.
			Migrate<ApplicationDbContext>(services, logger);
			Migrate<BlogContext>(services, logger);
			Migrate<ContentBlockContext>(services, logger);
		}
		catch (Exception ex)
		{
			logger.Error(ex, "An error occurred migrating CMS databases.");
			if (throwOnError)
			{
				throw;
			}
		}

		return app;
	}

	private static void Migrate<TContext>(IServiceProvider services, ILogger logger) where TContext : DbContext
	{
		var context = services.GetService<TContext>();
		if (context == null)
		{
			logger.Warning("DbContext {Context} not registered; skipping migrations.", typeof(TContext).Name);
			return;
		}
		var pending = context.Database.GetPendingMigrations().ToList();
		if (pending.Count == 0)
		{
			logger.Debug("No pending migrations for {Context}", typeof(TContext).Name);
		}
		else
		{
			logger.Information("Applying {Count} migrations for {Context}: {Migrations}", pending.Count, typeof(TContext).Name, string.Join(", ", pending));
		}
		context.Database.Migrate();
	}

	private static WebApplication EnsureCmsRolesAndAdminSeeded(this WebApplication app, bool throwOnError = false)
	{
		if (string.Equals(Environment.GetEnvironmentVariable("COMJUSTINSPICER_SKIP_ROLESEED"), "true", StringComparison.OrdinalIgnoreCase))
		{
			Log.ForContext(typeof(CMSExtensions)).Information("Skipping role/admin seeding due to COMJUSTINSPICER_SKIP_ROLESEED=true");
			return app;
		}

		using var scope = app.Services.CreateScope();
		var services = scope.ServiceProvider;
		var logger = Log.ForContext(typeof(CMSExtensions));

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

			var adminEmail = config["AdminUser:Email"];
			var adminPassword = config["AdminUser:Password"];
			if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
			{
				logger.Warning("Admin user not created - missing AdminUser:Email or AdminUser:Password configuration.");
				return app;
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
			Log.ForContext(typeof(CMSExtensions)).Error(ex, "An error occurred seeding roles/admin user.");
			if (throwOnError)
			{
				throw;
			}
		}

		return app;
	}

	private static WebApplication ConfigureMiddleware(this WebApplication app, bool throwOnError = false)
	{
		app.UseHsts();
		app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();
		app.MapRazorPages();

		return app;
	}
}