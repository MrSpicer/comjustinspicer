using Microsoft.AspNetCore.Mvc;
using comjustinspicer.Data.ContentBlock;
using Microsoft.AspNetCore.Authorization;

namespace comjustinspicer.Controllers;

[Authorize(Roles = "Admin")]
public class AdminContentBlockController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<AdminContentBlockController>();
    private readonly IContentBlockService _service;

    public AdminContentBlockController(IContentBlockService service)
    {
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
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            // new
            return View("ContentBlockUpsert", new Data.ContentBlock.Models.ContentBlockDTO());
        }

        var existing = await _service.GetByIdAsync(id.Value);

        if (existing == null)
        {
            existing = new Data.ContentBlock.Models.ContentBlockDTO();
        }

        return View("ContentBlockUpsert", existing);
    }

    [HttpPost("admin/contentblocks/edit/{id?}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Data.ContentBlock.Models.ContentBlockDTO model)
    {
        if (!ModelState.IsValid)
        {
            return View("ContentBlockUpsert", model);
        }

        var ok = await _service.UpsertAsync(model);
        if (!ok)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while saving the content block.");
            return View("ContentBlockUpsert", model);
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
