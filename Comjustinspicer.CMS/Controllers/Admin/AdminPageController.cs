using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Comjustinspicer.CMS.Models.Page;
using Comjustinspicer.CMS.Pages;

namespace Comjustinspicer.CMS.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminPageController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<AdminPageController>();
    private readonly IPageModel _model;
    private readonly IPageControllerRegistry _registry;

    public AdminPageController(
        IPageModel model,
        IPageControllerRegistry registry)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    [HttpGet("pages")]
    public async Task<IActionResult> Pages()
    {
        var vm = await _model.GetPageIndexAsync();
        return View("Pages", vm);
    }

    [HttpGet("pages/create")]
    public IActionResult CreatePage(string? parentRoute = null)
    {
        var vm = new PageUpsertViewModel();

        if (!string.IsNullOrWhiteSpace(parentRoute))
        {
            parentRoute = parentRoute.TrimEnd('/');
            if (!parentRoute.StartsWith('/'))
            {
                parentRoute = "/" + parentRoute;
            }

            vm.Route = parentRoute == "/" ? "/" : parentRoute + "/";
        }

        return View("PageUpsert", vm);
    }

    [HttpGet("pages/edit/{id}")]
    public async Task<IActionResult> EditPage(Guid id)
    {
        var vm = await _model.GetPageUpsertAsync(id);
        if (vm == null)
        {
            TempData["Error"] = "Page not found.";
            return RedirectToAction("Pages");
        }
        return View("PageUpsert", vm);
    }

    [HttpPost("pages/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePage(PageUpsertViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("PageUpsert", model);
        }

        var excludeId = model.Id.HasValue && model.Id != Guid.Empty ? model.Id : null;
        var routeAvailable = await _model.IsRouteAvailableAsync(model.Route, excludeId);
        if (!routeAvailable)
        {
            ModelState.AddModelError("Route", "This route is already in use by another page.");
            return View("PageUpsert", model);
        }

        var result = await _model.SavePageUpsertAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "An error occurred while saving the page.");
            return View("PageUpsert", model);
        }

        return RedirectToAction("Pages");
    }

    [HttpPost("pages/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePage(Guid id)
    {
        var ok = await _model.DeletePageAsync(id);
        if (!ok)
        {
            TempData["Error"] = "Could not delete page.";
        }

        return RedirectToAction("Pages");
    }

    #region Page Controller Registry

    [HttpGet("pages/controllers")]
    public IActionResult GetAllControllers()
    {
        var controllers = _registry.GetAllControllers().Select(c => new
        {
            name = c.Name,
            displayName = c.DisplayName,
            description = c.Description,
            category = c.Category
        }).ToList();

        return Json(controllers);
    }

    [HttpGet("pages/controllers/{name}/properties")]
    public IActionResult GetControllerProperties(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { error = "Controller name is required." });
        }

        var controller = _registry.GetByName(name);
        if (controller == null)
        {
            return NotFound(new { error = $"Controller '{name}' not found." });
        }

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

        return Json(new
        {
            controllerName = controller.Name,
            displayName = controller.DisplayName,
            category = controller.Category,
            properties
        });
    }

    #endregion
}
