using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;

namespace Comjustinspicer.CMS.Controllers.Api;

/// <summary>
/// API controller for content zone operations.
/// Used by the inline edit mode to add/update/delete zone items.
/// </summary>
[ApiController]
[Route("api/contentzones")]
[Authorize(Roles = "Admin")]
public class ContentZoneApiController : ControllerBase
{
    private readonly IContentZoneService _service;

    public ContentZoneApiController(IContentZoneService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>
    /// Add or update a content zone item.
    /// If the zone doesn't exist, it will be created automatically.
    /// </summary>
    [HttpPost("items")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveItem([FromBody] SaveItemRequest request, CancellationToken ct)
    {
        if (request == null)
            return BadRequest(new { error = "Request body is required." });

        if (string.IsNullOrWhiteSpace(request.ComponentName))
            return BadRequest(new { error = "Component name is required." });

        if (string.IsNullOrWhiteSpace(request.ZoneName))
            return BadRequest(new { error = "Zone name is required." });

        try
        {
            // Get or create the zone
            Guid zoneId;
            if (request.ZoneId.HasValue && request.ZoneId.Value != Guid.Empty)
            {
                zoneId = request.ZoneId.Value;
            }
            else
            {
                // Try to find existing zone by name, or create new one
                var existingZone = await _service.GetByNameAsync(request.ZoneName, ct);
                if (existingZone != null)
                {
                    zoneId = existingZone.Id;
                }
                else
                {
                    // Create new zone - ID is auto-generated
                    var newZone = new ContentZoneDTO
                    {
                        Id = Guid.NewGuid(),
                        Name = request.ZoneName,
                        Title = request.ZoneName,
                        IsPublished = true
                    };
                    var createdZone = await _service.CreateAsync(newZone, ct);
                    zoneId = createdZone.Id;
                }
            }

            // Create or update the item
            if (request.ItemId.HasValue && request.ItemId.Value != Guid.Empty)
            {
                // Update existing item
                var item = new ContentZoneItemDTO
                {
                    Id = request.ItemId.Value,
                    ContentZoneId = zoneId,
                    ComponentName = request.ComponentName,
                    ComponentPropertiesJson = request.ComponentPropertiesJson ?? "{}",
                    IsActive = true,
                    ModifiedAt = DateTime.UtcNow
                };

                var updated = await _service.UpdateItemAsync(item, ct);
                if (!updated)
                    return NotFound(new { error = "Item not found." });

                return Ok(new { success = true, itemId = item.Id, zoneId = zoneId });
            }
            else
            {
                // Create new item - ID is auto-generated
                var item = new ContentZoneItemDTO
                {
                    Id = Guid.NewGuid(),
                    ContentZoneId = zoneId,
                    ComponentName = request.ComponentName,
                    ComponentPropertiesJson = request.ComponentPropertiesJson ?? "{}",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdItem = await _service.AddItemAsync(zoneId, item, ct);
                return Ok(new { success = true, itemId = createdItem.Id, zoneId = zoneId });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to save item.", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a content zone item.
    /// </summary>
    [HttpDelete("items/{itemId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(Guid itemId, CancellationToken ct)
    {
        try
        {
            var deleted = await _service.RemoveItemAsync(itemId, ct);
            if (!deleted)
                return NotFound(new { error = "Item not found." });

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to delete item.", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific item for editing.
    /// </summary>
    [HttpGet("items/{itemId:guid}")]
    public async Task<IActionResult> GetItem(Guid itemId, CancellationToken ct)
    {
        var zone = await _service.GetAllAsync(ct);
        var item = zone.SelectMany(z => z.Items).FirstOrDefault(i => i.Id == itemId);

        if (item == null)
            return NotFound(new { error = "Item not found." });

        return Ok(new
        {
            id = item.Id,
            zoneId = item.ContentZoneId,
            componentName = item.ComponentName,
            componentPropertiesJson = item.ComponentPropertiesJson,
            ordinal = item.Ordinal,
            isActive = item.IsActive
        });
    }
}

/// <summary>
/// Request model for saving a content zone item.
/// </summary>
public class SaveItemRequest
{
    /// <summary>
    /// The zone path/name (used to find or create the zone).
    /// </summary>
    public string ZoneName { get; set; } = string.Empty;

    /// <summary>
    /// The zone ID if known (optional, zone will be looked up by name if not provided).
    /// This is set automatically and never displayed to users.
    /// </summary>
    public Guid? ZoneId { get; set; }

    /// <summary>
    /// The item ID if updating an existing item.
    /// This is set automatically and never displayed to users.
    /// </summary>
    public Guid? ItemId { get; set; }

    /// <summary>
    /// The name of the ViewComponent to render.
    /// </summary>
    public string ComponentName { get; set; } = string.Empty;

    /// <summary>
    /// JSON-serialized configuration properties for the component.
    /// </summary>
    public string? ComponentPropertiesJson { get; set; }
}
