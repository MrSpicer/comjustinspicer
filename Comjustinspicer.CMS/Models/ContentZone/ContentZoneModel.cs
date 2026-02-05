using System.Text.Json;
using Comjustinspicer.CMS.ContentZones;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;

namespace Comjustinspicer.CMS.Models.ContentZone;

public class ContentZoneModel : IContentZoneModel
{
    private readonly IContentZoneService _service;
    private readonly IContentZoneComponentRegistry _registry;

    public ContentZoneModel(IContentZoneService service, IContentZoneComponentRegistry registry)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
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

    private ContentZoneViewModel MapToViewModel(ContentZoneDTO zone)
    {
        var vm = new ContentZoneViewModel
        {
            Id = zone.Id,
            Name = zone.Name,
            ZoneObjects = zone.Items
                .OrderBy(i => i.Ordinal)
                .Select(i => new ContentZoneObject
                {
                    Id = i.Id,
                    Ordinal = i.Ordinal,
                    ZoneId = i.ContentZoneId,
                    ComponentName = i.ComponentName,
                    ComponentProperties = DeserializePropertiesToConfigType(i.ComponentName, i.ComponentPropertiesJson)
                })
                .ToList()
        };
        return vm;
    }

    /// <summary>
    /// Deserializes properties JSON into the actual configuration type for the component.
    /// Falls back to a dictionary if the component type is not registered.
    /// </summary>
    private object DeserializePropertiesToConfigType(string componentName, string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
        {
            // Try to create a default configuration instance
            var defaultConfig = _registry.CreateDefaultConfiguration(componentName);
            if (defaultConfig != null)
                return defaultConfig;
            return new { };
        }

        try
        {
            // Get the component info to find the configuration type
            var componentInfo = _registry.GetByName(componentName);
            if (componentInfo?.ConfigurationType != null)
            {
                // Deserialize to the actual configuration type
                var config = JsonSerializer.Deserialize(json, componentInfo.ConfigurationType, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (config != null)
                    return config;
            }

            // Fallback: deserialize to dictionary
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            return dict ?? new Dictionary<string, object>();
        }
        catch
        {
            return new { };
        }
    }
}
