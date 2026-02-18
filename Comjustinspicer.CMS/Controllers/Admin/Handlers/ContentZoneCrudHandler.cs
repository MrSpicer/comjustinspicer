using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Comjustinspicer.CMS.Attributes;
using Comjustinspicer.CMS.ContentZones;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Models.ContentZone;
using Comjustinspicer.CMS.Services;

namespace Comjustinspicer.CMS.Controllers.Admin.Handlers;

public class ContentZoneCrudHandler : IAdminCrudHandler
{
    private readonly IContentZoneModel _model;
    private readonly ContentZoneChildHandler _childHandler;

    public ContentZoneCrudHandler(
        IContentZoneModel model,
        IContentZoneComponentRegistry registry,
        IViewComponentViewDiscoveryService viewDiscoveryService)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _childHandler = new ContentZoneChildHandler(model);
        RegistryHandler = new ContentZoneRegistryHandler(
            registry ?? throw new ArgumentNullException(nameof(registry)),
            viewDiscoveryService ?? throw new ArgumentNullException(nameof(viewDiscoveryService)));
    }

    public string ContentType => "contentzones";
    public string DisplayName => "Content Zone";
    public string[]? WriteRoles => null;

    public string IndexViewPath => "~/Views/AdminContentZone/ContentZones.cshtml";
    public string UpsertViewPath => ""; // No upsert view currently exists

    public async Task<object> GetIndexViewModelAsync(CancellationToken ct = default)
        => await _model.GetAllAsync(ct);

    public Task<object?> GetUpsertViewModelAsync(Guid? id, IQueryCollection query, CancellationToken ct = default)
        => Task.FromResult<object?>(null); // Not currently implemented

    public object CreateEmptyUpsertViewModel() => new ContentZoneDTO();

    public Task<AdminSaveResult> SaveUpsertAsync(object model, CancellationToken ct = default)
        => Task.FromResult(new AdminSaveResult(false, "Content zone upsert is not implemented via this interface."));

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => _model.DeleteAsync(id, ct);

    public async Task<IEnumerable<object>> GetApiListAsync(CancellationToken ct = default)
    {
        var zones = await _model.GetAllAsync(ct);
        return zones.Select(z => (object)new { id = z.Id, title = !string.IsNullOrEmpty(z.Title) ? z.Title : z.Name });
    }

    public bool HasSecondaryApiList => false;

    public Task<IEnumerable<object>> GetSecondaryApiListAsync(string key, CancellationToken ct = default)
        => Task.FromResult(Enumerable.Empty<object>());

    public IAdminRegistryHandler? RegistryHandler { get; }
    public IAdminCrudChildHandler? ChildHandler => _childHandler;
}

/// <summary>Manages items within a content zone (child entities).</summary>
internal sealed class ContentZoneChildHandler : IAdminCrudChildHandler
{
    private readonly IContentZoneModel _model;

    public ContentZoneChildHandler(IContentZoneModel model)
    {
        _model = model;
    }

    public string ChildType => "items";
    public string ChildDisplayName => "Content Zone Item";
    public string[]? WriteRoles => null;

    public string ChildIndexViewPath => "~/Views/AdminContentZone/ContentZoneItems.cshtml";
    public string ChildUpsertViewPath => ""; // Item add/edit is handled via JavaScript modal

    public async Task<object?> GetChildIndexViewModelAsync(string parentKey, CancellationToken ct = default)
    {
        if (!Guid.TryParse(parentKey, out var zoneId)) return null;
        return await _model.GetByIdAsync(zoneId, ct);
    }

    public Task<object?> GetChildUpsertViewModelAsync(string parentKey, Guid? id, CancellationToken ct = default)
        => Task.FromResult<object?>(null); // Managed via JavaScript modal

    public Task SetChildUpsertViewDataAsync(ViewDataDictionary viewData, string parentKey, CancellationToken ct = default)
        => Task.CompletedTask;

    public object CreateEmptyChildUpsertViewModel() => new ContentZoneItemDTO();

    public Task<AdminSaveResult> SaveChildUpsertAsync(string parentKey, object model, CancellationToken ct = default)
        => Task.FromResult(new AdminSaveResult(false, "Content zone item save is handled via the API controller."));

    public Task<bool> DeleteChildAsync(Guid id, CancellationToken ct = default)
        => _model.RemoveItemAsync(id, ct);

    public bool SupportsReorder => false;

    public Task<bool> ReorderAsync(string parentKey, List<Guid> orderedIds, CancellationToken ct = default)
        => Task.FromResult(false);
}

/// <summary>Exposes the content zone component registry as admin JSON endpoints.</summary>
internal sealed class ContentZoneRegistryHandler : IAdminRegistryHandler
{
    private readonly IContentZoneComponentRegistry _registry;
    private readonly IViewComponentViewDiscoveryService _viewDiscoveryService;
    private readonly Serilog.ILogger _logger =
        Serilog.Log.ForContext<ContentZoneRegistryHandler>();

    public ContentZoneRegistryHandler(
        IContentZoneComponentRegistry registry,
        IViewComponentViewDiscoveryService viewDiscoveryService)
    {
        _registry = registry;
        _viewDiscoveryService = viewDiscoveryService;
    }

    public IActionResult GetAll()
    {
        var components = _registry.GetAllComponents().Select(c => new
        {
            name = c.Name,
            displayName = c.DisplayName,
            description = c.Description,
            category = c.Category
        }).ToList();

        return new JsonResult(components);
    }

    public IActionResult GetProperties(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new BadRequestObjectResult(new { error = "Component name is required." });

        var component = _registry.GetByName(name);
        if (component == null)
            return new NotFoundObjectResult(new { error = $"Component '{name}' not found." });

        var properties = component.Properties.Select(p =>
        {
            Dictionary<string, string> dropdownOptions = p.DropdownOptions;

            if (p.EditorType == EditorType.ViewPicker && !string.IsNullOrWhiteSpace(p.ViewComponentName))
            {
                var views = _viewDiscoveryService.GetAvailableViews(p.ViewComponentName);
                if (views.Any())
                    dropdownOptions = views.ToDictionary(v => v, v => v);
                else
                {
                    _logger.Warning("No views found for ViewComponent '{ComponentName}'", p.ViewComponentName);
                    dropdownOptions = new Dictionary<string, string>();
                }
            }

            return new
            {
                name = p.Name,
                label = p.Label,
                helpText = p.HelpText,
                placeholder = p.Placeholder,
                editorType = p.EditorType.ToString().ToLowerInvariant(),
                isRequired = p.IsRequired,
                defaultValue = p.DefaultValue,
                order = p.Order,
                group = p.Group,
                entityType = p.EntityType,
                dropdownOptions,
                viewComponentName = p.ViewComponentName,
                min = p.Min,
                max = p.Max,
                maxLength = p.MaxLength
            };
        }).OrderBy(p => p.order).ToList();

        return new JsonResult(new
        {
            componentName = component.Name,
            displayName = component.DisplayName,
            category = component.Category,
            properties
        });
    }
}
