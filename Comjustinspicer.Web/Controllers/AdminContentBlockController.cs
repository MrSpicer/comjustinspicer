using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.Data.ContentBlock;
using Microsoft.AspNetCore.Authorization;
using Comjustinspicer.Models.ContentBlock;

namespace Comjustinspicer.Controllers;

[Authorize(Roles = "Admin")]
public class AdminContentBlockController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<AdminContentBlockController>();
    private readonly IContentBlockService _service;
    private readonly ContentBlockModel _model;

    //todo: all of the logic here needs to be moved into the ContentBlockModel class
    public AdminContentBlockController(ContentBlockModel model, IContentBlockService service)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    // List all content blocks for admin management
    [HttpGet("admin/contentblocks")]
    public async Task<IActionResult> Index()
    {
        var all = await _service.GetAllAsync();
        return View("ContentBlocks", all);
    }

    [HttpGet("admin/contentblocks/edit/{id?}")]
    public async Task<IActionResult> Edit(Guid? id, string? returnUrl)
    {
        if (id == null)
        {
            // new
            if (!string.IsNullOrWhiteSpace(returnUrl)) ViewData["ReturnUrl"] = returnUrl;
            return View("ContentBlockUpsert", new Data.ContentBlock.Models.ContentBlockDTO());
        }

        var existing = await _service.GetByIdAsync(id.Value);

        if (existing == null)
        {
            existing = new Data.ContentBlock.Models.ContentBlockDTO();
        }

        if (!string.IsNullOrWhiteSpace(returnUrl)) ViewData["ReturnUrl"] = returnUrl;

        return View("ContentBlockUpsert", existing);
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

        var ok = await _service.UpsertAsync(model);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while saving the content block.");
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
        var ok = await _service.DeleteAsync(id);
        if (!ok)
        {
            TempData["Error"] = "Could not delete content block.";
        }

        return RedirectToAction("Index");
    }
}
