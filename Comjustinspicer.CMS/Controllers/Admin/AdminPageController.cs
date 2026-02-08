using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Comjustinspicer.CMS.Models.Page;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using Comjustinspicer.CMS.Pages;

namespace Comjustinspicer.CMS.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminPageController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<AdminPageController>();
    private readonly IPageModel _model;
    private readonly IPageService _pageService;
    private readonly IPageControllerRegistry _registry;

    public AdminPageController(
        IPageModel model,
        IPageService pageService,
        IPageControllerRegistry registry)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    [HttpGet("pages")]
    public async Task<IActionResult> Pages()
    {
        var tree = await _model.GetRouteTreeAsync();
        return View("Pages", tree);
    }

    [HttpGet("pages/create")]
    public IActionResult CreatePage(string? parentRoute = null)
    {
        var vm = new PageDTO();

        if (!string.IsNullOrWhiteSpace(parentRoute))
        {
            // Normalize parent route
            parentRoute = parentRoute.TrimEnd('/');
            if (!parentRoute.StartsWith('/'))
            {
                parentRoute = "/" + parentRoute;
            }

            // Set initial route as parent + /
            vm.Route = parentRoute == "/" ? "/" : parentRoute + "/";
        }

        return View("PageUpsert", vm);
    }

    [HttpGet("pages/edit/{id}")]
    public async Task<IActionResult> EditPage(Guid id)
    {
        var page = await _model.GetByIdAsync(id);
        if (page == null)
        {
            TempData["Error"] = "Page not found.";
            return RedirectToAction("Pages");
        }
        return View("PageUpsert", page);
    }

    [HttpPost("pages/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePage(PageDTO model)
    {
        if (!ModelState.IsValid)
        {
            return View("PageUpsert", model);
        }

        // Validate route uniqueness
        var excludeId = model.Id != Guid.Empty ? model.Id : (Guid?)null;
        var routeAvailable = await _pageService.IsRouteAvailableAsync(model.Route, excludeId);
        if (!routeAvailable)
        {
            ModelState.AddModelError("Route", "This route is already in use by another page.");
            return View("PageUpsert", model);
        }

        // Populate user audit fields
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userId, out var currentUserId))
        {
            if (model.Id == Guid.Empty)
            {
                model.CreatedBy = currentUserId;
            }
            model.LastModifiedBy = currentUserId;
        }

        if (model.Id == Guid.Empty)
        {
            await _model.CreateAsync(model);
        }
        else
        {
            var updated = await _model.UpdateAsync(model);
            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "Failed to update page.");
                return View("PageUpsert", model);
            }
        }

        return RedirectToAction("Pages");
    }

    [HttpPost("pages/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePage(Guid id)
    {
        var ok = await _model.DeleteAsync(id);
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
