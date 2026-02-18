using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.CMS.Models.Page;
using Comjustinspicer.CMS.Pages;

namespace Comjustinspicer.CMS.Controllers.Admin.Handlers;

public class PageCrudHandler : IAdminCrudHandler
{
    private readonly IPageModel _model;

    public PageCrudHandler(IPageModel model, IPageControllerRegistry registry)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        RegistryHandler = new PageRegistryHandler(registry ?? throw new ArgumentNullException(nameof(registry)));
    }

    public string ContentType => "pages";
    public string DisplayName => "Page";
    public string[]? WriteRoles => null;

    public string IndexViewPath => "~/Views/AdminPage/Pages.cshtml";
    public string UpsertViewPath => "~/Views/AdminPage/PageUpsert.cshtml";

    public async Task<object> GetIndexViewModelAsync(CancellationToken ct = default)
        => await _model.GetPageIndexAsync(ct);

    public async Task<object?> GetUpsertViewModelAsync(Guid? id, IQueryCollection query, CancellationToken ct = default)
    {
        if (id.HasValue && id != Guid.Empty)
        {
            var existing = await _model.GetPageUpsertAsync(id, ct);
            if (existing == null) return null;
            return existing;
        }

        // Create — optionally pre-fill Route from parentRoute query parameter
        var vm = new PageUpsertViewModel();
        var parentRoute = query["parentRoute"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(parentRoute))
        {
            parentRoute = parentRoute.TrimEnd('/');
            if (!parentRoute.StartsWith('/'))
                parentRoute = "/" + parentRoute;
            vm.Route = parentRoute == "/" ? "/" : parentRoute + "/";
        }
        return vm;
    }

    public object CreateEmptyUpsertViewModel() => new PageUpsertViewModel();

    public async Task<AdminSaveResult> SaveUpsertAsync(object model, CancellationToken ct = default)
    {
        var vm = (PageUpsertViewModel)model;

        // Page-specific: route uniqueness validation
        var excludeId = vm.Id.HasValue && vm.Id != Guid.Empty ? vm.Id : null;
        var routeAvailable = await _model.IsRouteAvailableAsync(vm.Route, excludeId, ct);
        if (!routeAvailable)
            return new AdminSaveResult(false, "This route is already in use by another page.", "Route");

        var result = await _model.SavePageUpsertAsync(vm, ct);
        return result.Success
            ? new AdminSaveResult(true)
            : new AdminSaveResult(false, result.ErrorMessage);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => _model.DeletePageAsync(id, ct);

    public Task<IEnumerable<object>> GetApiListAsync(CancellationToken ct = default)
        => Task.FromResult(Enumerable.Empty<object>());

    public bool HasSecondaryApiList => false;

    public Task<IEnumerable<object>> GetSecondaryApiListAsync(string key, CancellationToken ct = default)
        => Task.FromResult(Enumerable.Empty<object>());

    public IAdminRegistryHandler? RegistryHandler { get; }
    public IAdminCrudChildHandler? ChildHandler => null;
}

internal sealed class PageRegistryHandler : IAdminRegistryHandler
{
    private readonly IPageControllerRegistry _registry;

    public PageRegistryHandler(IPageControllerRegistry registry)
    {
        _registry = registry;
    }

    public IActionResult GetAll()
    {
        var controllers = _registry.GetAllControllers().Select(c => new
        {
            name = c.Name,
            displayName = c.DisplayName,
            description = c.Description,
            category = c.Category
        }).ToList();

        return new JsonResult(controllers);
    }

    public IActionResult GetProperties(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new BadRequestObjectResult(new { error = "Controller name is required." });

        var controller = _registry.GetByName(name);
        if (controller == null)
            return new NotFoundObjectResult(new { error = $"Controller '{name}' not found." });

        var properties = controller.Properties.Select(p => new
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
            dropdownOptions = p.DropdownOptions,
            viewComponentName = p.ViewComponentName,
            min = p.Min,
            max = p.Max,
            maxLength = p.MaxLength
        }).OrderBy(p => p.order).ToList();

        return new JsonResult(new
        {
            controllerName = controller.Name,
            displayName = controller.DisplayName,
            category = controller.Category,
            properties
        });
    }
}
