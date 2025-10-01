using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.Data.ContentBlock;
using Microsoft.AspNetCore.Authorization;
using Comjustinspicer.Models.ContentBlock;

namespace Comjustinspicer.Controllers;

[Authorize(Roles = "Admin")]
public class AdminContentBlockController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<AdminContentBlockController>();
    private readonly IContentBlockModel _model;

    // Business logic moved into ContentBlockModel
    public AdminContentBlockController(IContentBlockModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    // List all content blocks for admin management
    [HttpGet("admin/contentblocks")]
    public async Task<IActionResult> Index()
    {
        var all = await _model.GetAllAsync();
        return View("ContentBlocks", all);
    }

    [HttpGet("admin/contentblocks/edit/{id?}")]
    public async Task<IActionResult> Edit(Guid? id, string? returnUrl)
    {
        var vm = await _model.GetUpsertModelAsync(id);
        if (!string.IsNullOrWhiteSpace(returnUrl)) ViewData["ReturnUrl"] = returnUrl;
        return View("ContentBlockUpsert", vm);
    }

    [HttpPost("admin/contentblocks/edit/{id?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Data.ContentBlock.Models.ContentBlockDTO model, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl)) ViewData["ReturnUrl"] = returnUrl;
            return View("ContentBlockUpsert", model);
        }

        var result = await _model.SaveUpsertAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "An error occurred while saving the content block.");
            return View("ContentBlockUpsert", model);
        }

        // If a returnUrl was supplied and it's a local url, redirect there instead of Index
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index");
    }

    [HttpPost("admin/contentblocks/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _model.DeleteAsync(id);
        if (!ok)
        {
            TempData["Error"] = "Could not delete content block.";
        }

        return RedirectToAction("Index");
    }
}
