using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Comjustinspicer.CMS.Models.ContentBlock;

namespace Comjustinspicer.CMS.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin")]
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
    [HttpGet("contentblocks")]
    public async Task<IActionResult> Index()
    {
        var all = await _model.GetAllAsync();
        return View("ContentBlocks", all);
    }

    [HttpGet("contentblocks/edit/{id?}")]
    public async Task<IActionResult> Edit(Guid? id, string? returnUrl)
    {
        var vm = await _model.GetUpsertModelAsync(id);
        if (!string.IsNullOrWhiteSpace(returnUrl)) ViewData["ReturnUrl"] = returnUrl;
        return View("ContentBlockUpsert", vm);
    }

    [HttpPost("contentblocks/edit/{id?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ContentBlockUpsertViewModel model, string? returnUrl)
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

    [HttpPost("contentblocks/delete/{id}")]
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

    /// <summary>
    /// API endpoint to list content blocks for entity pickers.
    /// </summary>
    [HttpGet("contentblocks/api/list")]
    public async Task<IActionResult> ApiList()
    {
        var all = await _model.GetAllAsync();
        var result = all.Select(cb => new { id = cb.Id, title = cb.Title }).ToList();
        return Json(result);
    }
}
