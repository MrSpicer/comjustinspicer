using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Comjustinspicer.CMS.Data.Database;
using Comjustinspicer.CMS.Data;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using Comjustinspicer.CMS.Models.ContentBlock;
using Comjustinspicer.CMS.Data.DbContexts;


namespace Comjustinspicer.CMS;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers CMS services and configures EF Core DbContexts using the provided configuration.
	/// </summary>
	/// <param name="services">The DI service collection.</param>
	/// <param name="configuration">Application configuration for resolving connection string.</param>
	/// <returns>The same service collection for chaining.</returns>
	public static IServiceCollection AddComjustinspicerCms(this IServiceCollection services, IConfiguration configuration)
	{
		ConfigureDatabaseServices(services, configuration);
		MapTypes(services);
		ConfigureAuthorization(services);
		return services;
	}

	private static void ConfigureDatabaseServices(IServiceCollection services, IConfiguration configuration)
	{

		var configurator = typeof(ServiceCollectionExtensions).Assembly
			.GetTypes()
			.Where(t => !t.IsAbstract && typeof(IDatabaseConfigurator).IsAssignableFrom(t))
			.Select(t => Activator.CreateInstance(t) as IDatabaseConfigurator)
			.FirstOrDefault(c => c != null && c.DBTypeSupported == configuration["DatabaseType"]);

		if (configurator is null)
		{
			throw new InvalidOperationException($"No database configurator found for type '{configuration["DatabaseType"]}'.");
		}

		configurator.Configure(services, configuration);

		services.AddDatabaseDeveloperPageExceptionFilter();

	}

	private static void MapTypes(IServiceCollection services)
	{
#if DEBUG
		services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, Services.DevEmailSender>();
#endif
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