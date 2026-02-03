using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Comjustinspicer.CMS.Models.ContentZone;
using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminContentZoneController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<AdminContentZoneController>();
    private readonly IContentZoneModel _model;

    public AdminContentZoneController(IContentZoneModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    #region Content Zones

    // List all content zones
    [HttpGet("contentzones")]
    public async Task<IActionResult> Index()
    {
        var all = await _model.GetAllAsync();
        return View("ContentZones", all);
    }

    // Create new content zone form
    [HttpGet("contentzones/create")]
    public IActionResult Create()
    {
        var vm = new ContentZoneDTO();
        return View("ContentZoneUpsert", vm);
    }

    // Edit content zone form
    [HttpGet("contentzones/edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var zone = await _model.GetByIdAsync(id);
        if (zone == null)
        {
            TempData["Error"] = "Content zone not found.";
            return RedirectToAction("Index");
        }
        return View("ContentZoneUpsert", zone);
    }

    // Save content zone (create or update)
    [HttpPost("contentzones/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ContentZoneDTO model)
    {
        if (!ModelState.IsValid)
        {
            return View("ContentZoneUpsert", model);
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
                ModelState.AddModelError(string.Empty, "Failed to update content zone.");
                return View("ContentZoneUpsert", model);
            }
        }

        return RedirectToAction("Index");
    }

    // Delete content zone
    [HttpPost("contentzones/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _model.DeleteAsync(id);
        if (!ok)
        {
            TempData["Error"] = "Could not delete content zone.";
        }

        return RedirectToAction("Index");
    }

    // View/manage items in a content zone
    [HttpGet("contentzones/{zoneId}/items")]
    public async Task<IActionResult> Items(Guid zoneId)
    {
        var zone = await _model.GetByIdAsync(zoneId);
        if (zone == null)
        {
            TempData["Error"] = "Content zone not found.";
            return RedirectToAction("Index");
        }

        return View("ContentZoneItems", zone);
    }

    #endregion

    #region Content Zone Items

    // Add item to zone form
    [HttpGet("contentzones/{zoneId}/items/add")]
    public IActionResult AddItem(Guid zoneId)
    {
        var vm = new ContentZoneItemDTO { ContentZoneId = zoneId };
        ViewData["ZoneId"] = zoneId;
        return View("ContentZoneItemUpsert", vm);
    }

    // Edit item form
    [HttpGet("contentzones/items/edit/{itemId}")]
    public async Task<IActionResult> EditItem(Guid itemId)
    {
        var item = await _model.GetItemByIdAsync(itemId);
        if (item == null)
        {
            TempData["Error"] = "Content zone item not found.";
            return RedirectToAction("Index");
        }

        ViewData["ZoneId"] = item.ContentZoneId;
        return View("ContentZoneItemUpsert", item);
    }

    // Save item (create or update)
    [HttpPost("contentzones/items/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveItem(ContentZoneItemDTO model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ZoneId"] = model.ContentZoneId;
            return View("ContentZoneItemUpsert", model);
        }

        if (model.Id == Guid.Empty)
        {
            await _model.AddItemAsync(model.ContentZoneId, model);
        }
        else
        {
            var updated = await _model.UpdateItemAsync(model);
            if (!updated)
            {
                ModelState.AddModelError(string.Empty, "Failed to update item.");
                ViewData["ZoneId"] = model.ContentZoneId;
                return View("ContentZoneItemUpsert", model);
            }
        }

        return RedirectToAction("Items", new { zoneId = model.ContentZoneId });
    }

    // Remove item from zone
    [HttpPost("contentzones/items/delete/{itemId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(Guid itemId, Guid zoneId)
    {
        var ok = await _model.RemoveItemAsync(itemId);
        if (!ok)
        {
            TempData["Error"] = "Could not delete item.";
        }

        return RedirectToAction("Items", new { zoneId });
    }

    // Reorder items (AJAX endpoint)
    [HttpPost("contentzones/{zoneId}/items/reorder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReorderItems(Guid zoneId, [FromBody] List<Guid> itemIds)
    {
        if (itemIds == null || !itemIds.Any())
        {
            return BadRequest("No items provided.");
        }

        var ok = await _model.ReorderItemsAsync(zoneId, itemIds);
        if (!ok)
        {
            return StatusCode(500, "Failed to reorder items.");
        }

        return Ok();
    }

    #endregion
}
