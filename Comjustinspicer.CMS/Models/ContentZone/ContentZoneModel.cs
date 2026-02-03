using System.Text.Json;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;

namespace Comjustinspicer.CMS.Models.ContentZone;

public class ContentZoneModel : IContentZoneModel
{
    private readonly IContentZoneService _service;

    public ContentZoneModel(IContentZoneService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async Task<ContentZoneViewModel?> GetViewModelAsync(string contentZoneName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(contentZoneName))
            return null;

        var zone = await _service.GetByNameAsync(contentZoneName, ct);
        if (zone == null)
            return new ContentZoneViewModel { Name = contentZoneName };

        return MapToViewModel(zone);
    }

    public async Task<ContentZoneDTO?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _service.GetByIdAsync(id, ct);
    }

    public async Task<List<ContentZoneDTO>> GetAllAsync(CancellationToken ct = default)
    {
        return await _service.GetAllAsync(ct);
    }

    public async Task<ContentZoneDTO> CreateAsync(ContentZoneDTO zone, CancellationToken ct = default)
    {
        return await _service.CreateAsync(zone, ct);
    }

    public async Task<bool> UpdateAsync(ContentZoneDTO zone, CancellationToken ct = default)
    {
        return await _service.UpdateAsync(zone, ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return await _service.DeleteAsync(id, ct);
    }

    public async Task<ContentZoneItemDTO> AddItemAsync(Guid zoneId, ContentZoneItemDTO item, CancellationToken ct = default)
    {
        return await _service.AddItemAsync(zoneId, item, ct);
    }

    public async Task<bool> UpdateItemAsync(ContentZoneItemDTO item, CancellationToken ct = default)
    {
        return await _service.UpdateItemAsync(item, ct);
    }

    public async Task<bool> RemoveItemAsync(Guid itemId, CancellationToken ct = default)
    {
        return await _service.RemoveItemAsync(itemId, ct);
    }

    public async Task<ContentZoneItemDTO?> GetItemByIdAsync(Guid itemId, CancellationToken ct = default)
    {
        return await _service.GetItemByIdAsync(itemId, ct);
    }

    public async Task<bool> ReorderItemsAsync(Guid zoneId, List<Guid> itemIdsInOrder, CancellationToken ct = default)
    {
        return await _service.ReorderItemsAsync(zoneId, itemIdsInOrder, ct);
    }

    private static ContentZoneViewModel MapToViewModel(ContentZoneDTO zone)
    {
        var vm = new ContentZoneViewModel
        {
            Name = zone.Name,
            ZoneObjects = zone.Items
                .OrderBy(i => i.Ordinal)
                .Select(i => new ContentZoneObject
                {
                    Ordinal = i.Ordinal,
                    ZoneId = i.ContentZoneId,
                    ComponentName = i.ComponentName,
                    ComponentProperties = DeserializeProperties(i.ComponentPropertiesJson)
                })
                .ToList()
        };
        return vm;
    }

    private static object DeserializeProperties(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return new { };

        try
        {
            // Deserialize to dictionary for passing to ViewComponent
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            return dict ?? new Dictionary<string, object>();
        }
        catch
        {
            return new { };
        }
    }
}
