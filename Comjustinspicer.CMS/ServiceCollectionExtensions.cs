using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AutoMapper;
using Comjustinspicer.CMS.Data;
using Comjustinspicer.CMS.Data.Blog;
using Comjustinspicer.CMS.Data.ContentBlock;
using Comjustinspicer.CMS.Data.Blog.Models;
using Comjustinspicer.CMS.Data.ContentBlock.Models;
using Comjustinspicer.CMS.Data.Services;
// using Comjustinspicer.CMS.Models.Blog;
using Comjustinspicer.CMS.Models.ContentBlock;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Comjustinspicer.CMS.Data.DbContexts;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Comjustinspicer.CMS;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers CMS (Data, Models, Services) components and adds MVC application part for Controllers/ViewComponents.
	/// </summary>
	public static IServiceCollection AddComjustinspicerCms(this IServiceCollection services)
	{
		// Backwards-compatible overload assumes database contexts already configured by host.
		AddCmsCore(services);
		return services;
	}

	/// <summary>
	/// Registers CMS services and configures EF Core DbContexts using the provided configuration.
	/// </summary>
	/// <param name="services">The DI service collection.</param>
	/// <param name="configuration">Application configuration for resolving connection string.</param>
	/// <returns>The same service collection for chaining.</returns>
	public static IServiceCollection AddComjustinspicerCms(this IServiceCollection services, IConfiguration configuration)
	{
		ConfigureDatabaseServices(services, configuration);
		AddCmsCore(services);
		return services;
	}

	private static void AddCmsCore(IServiceCollection services)
	{
		MapTypes(services);
		ConfigureAuthorization(services);
	}

	private static void ConfigureDatabaseServices(IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection")
			?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

		// Main application DB (Identity + app data)
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlite(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory_Application")));

		// Blog DB/context can share the same connection or be configured separately in appsettings
		services.AddDbContext<BlogContext>(options =>
			options.UseSqlite(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory_Blog")));

		// ContentBlock DB/context
		services.AddDbContext<ContentBlockContext>(options =>
			options.UseSqlite(connectionString, b => b.MigrationsHistoryTable("__EFMigrationsHistory_ContentBlock")));

		services.AddDatabaseDeveloperPageExceptionFilter();
	}

	private static void MapTypes(IServiceCollection services)
	{
#if DEBUG
		services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, Services.DevEmailSender>();
#endif
		// Needed for UserService to inspect current HttpContext/User
		services.AddHttpContextAccessor();
		services.AddSingleton<Services.UserService>();

		// Generic content service registrations to enable consumers to request IContentService<T>
		// Note: Each T must be bound to the correct DbContext through constructor injection of DbContext.
		services.AddScoped<IContentService<PostDTO>>(sp =>
		{
			var ctx = sp.GetRequiredService<BlogContext>();
			return new ContentService<PostDTO>(ctx);
		});
		services.AddScoped<IContentService<ContentBlockDTO>>(sp =>
		{
			var ctx = sp.GetRequiredService<ContentBlockContext>();
			return new ContentService<ContentBlockDTO>(ctx);
		});

		services.AddScoped<IContentBlockModel, ContentBlockModel>();

		// AutoMapper profile from this assembly
		services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>(), typeof(ServiceCollectionExtensions).Assembly);

		// Register this assembly so that controllers/view components & embedded views are discovered
		services.Configure<MvcOptions>(_ => { }); // no-op to ensure MVC services available if host only calls minimal AddControllersWithViews later
		services.AddControllersWithViews().ConfigureApplicationPartManager(apm =>
		{
			var asm = typeof(ServiceCollectionExtensions).Assembly;
			if (!apm.ApplicationParts.Any(p => p.Name == asm.GetName().Name))
			{
				apm.ApplicationParts.Add(new AssemblyPart(asm));
			}
		});
	}

	static void ConfigureAuthorization(IServiceCollection services)
	{
		// Identity and authentication
		services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultUI();
	}
}