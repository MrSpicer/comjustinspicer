using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Comjustinspicer.CMS.ContentZones;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Models.ContentZone;

namespace Comjustinspicer.CMS.ViewComponents;

/// <summary>
/// ViewComponent that renders a content zone by path.
/// Content zones contain a list of other view components configured in the database.
/// The path is auto-generated from context or can be explicitly provided.
/// When an admin user is viewing, an edit mode is displayed allowing inline management.
/// </summary>
public class ContentZoneViewComponent : ViewComponent
{
	private readonly IContentZoneModel _model;
	private readonly IContentZoneComponentRegistry _registry;

	public ContentZoneViewComponent(IContentZoneModel model, IContentZoneComponentRegistry registry)
	{
		_model = model ?? throw new ArgumentNullException(nameof(model));
		_registry = registry ?? throw new ArgumentNullException(nameof(registry));
	}

	/// <summary>
	/// Renders the content zone with the specified path.
	/// </summary>
	/// <param name="zonePath">
	/// The path identifier for the content zone. If not provided, it will be auto-generated
	/// from the current route and zone name.
	/// </param>
	/// <param name="zoneName">
	/// The zone name within the current context. Used to build the path if zonePath is not provided.
	/// </param>
	/// <param name="parentPath">
	/// Optional parent path for nested content zones (e.g., when a zone is inside another ViewComponent).
	/// </param>
	/// <returns>The rendered view containing all zone items.</returns>
	public async Task<IViewComponentResult> InvokeAsync(
		string? zonePath = null,
		string? zoneName = null,
		string? parentPath = null,
		bool IsGlobal = false)
	{
		// Build the effective path
		var effectivePath = BuildZonePath(zonePath, zoneName, parentPath, IsGlobal);

		if (string.IsNullOrWhiteSpace(effectivePath))
			return Content(string.Empty);

		// Check if user is an admin
		var isAdmin = HttpContext.User?.IsInRole("Admin") == true;

		// Try to get the zone - it may not exist yet
		var vm = await _model.GetViewModelAsync(effectivePath, HttpContext.RequestAborted);

		if (vm == null)
		{
			// Zone doesn't exist - create an empty view model for edit mode
			vm = new ContentZoneViewModel
			{
				Id = Guid.Empty,
				Name = effectivePath,
				RawZoneName = zoneName ?? string.Empty,
				ZoneObjects = new List<ContentZoneObject>(),
				CanEdit = isAdmin
			};
		}
		else
		{
			vm.CanEdit = isAdmin;
			vm.RawZoneName = zoneName ?? string.Empty;
		}

		// Store the current path in ViewData for nested zones to access
		ViewData["ContentZone:ParentPath"] = effectivePath;

		// If admin, show edit mode with inline editing capabilities
		if (isAdmin)
		{
			// Pass component registry data for the modal
			ViewData["ComponentsByCategory"] = _registry.GetComponentsByCategory();
			return View("Edit", vm);
		}

		// Normal mode - only render if there are items
		if (vm.ZoneObjects?.Any() != true)
			return Content(string.Empty);

		return View(vm);
	}

	/// <summary>
	/// Builds the effective zone path from the available context.
	/// Uses render-position ordinals to ensure each zone instance is unique,
	/// even when the same component is rendered multiple times on a page.
	/// </summary>
	private string BuildZonePath(string? zonePath, string? zoneName, string? parentPath, bool IsGlobal = false)
	{
		// Determine the parent context for this zone
		var inheritedParentPath = parentPath ?? ViewData["ContentZone:ParentPath"] as string;
		var parentContext = inheritedParentPath ?? GetRoutePath(IsGlobal);

		// Get the next ordinal for zones under this parent
		var ordinal = GetNextOrdinal(parentContext);

		// If explicit path provided, append ordinal for uniqueness
		if (!string.IsNullOrWhiteSpace(zonePath))
			return $"{zonePath}#{ordinal}";

		// If no zone name, we can't build a path
		if (string.IsNullOrWhiteSpace(zoneName))
			return string.Empty;

		// Build path with ordinal: parentContext/zoneName#ordinal
		return $"{parentContext}/{zoneName}#{ordinal}";
	}

	/// <summary>
	/// Gets the next ordinal number for zones rendered under the specified parent.
	/// This ensures each zone instance gets a unique position-based identifier.
	/// </summary>
	private int GetNextOrdinal(string parentContext)
	{
		var counterKey = $"ContentZone:Ordinal:{parentContext}";
		var currentOrdinal = ViewData[counterKey] as int? ?? 0;
		ViewData[counterKey] = currentOrdinal + 1;
		return currentOrdinal;
	}

	/// <summary>
	/// Gets the current route path for zone identification.
	/// </summary>
	private string GetRoutePath(bool IsGlobal)
	{
		// If we're in a page context, use the page's unique ID for zone scoping
		if (!IsGlobal && HttpContext.Items["CMS:PageData"] is PageDTO pageData)
			return $"page:{pageData.Id}";

		// Fallback for non-page contexts (layout zones, admin pages, etc.)
		var routeData = HttpContext.GetRouteData();
		var controller = routeData.Values["controller"]?.ToString() ?? string.Empty;
		var action = routeData.Values["action"]?.ToString() ?? string.Empty;

		if (string.IsNullOrEmpty(controller))
		{
			var path = HttpContext.Request.Path.Value?.Trim('/') ?? string.Empty;
			return string.IsNullOrEmpty(path) ? "Home" : path.Replace("/", "_");
		}

		return $"{controller}/{action}";
	}
}
