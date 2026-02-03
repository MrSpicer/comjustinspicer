using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Data.Services;

/// <summary>
/// Service interface for ContentZone-specific operations beyond generic CRUD.
/// </summary>
public interface IContentZoneService
{
    /// <summary>
    /// Gets a content zone by its unique name, including all active items.
    /// </summary>
    Task<ContentZoneDTO?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Gets a content zone by ID, including all items.
    /// </summary>
    Task<ContentZoneDTO?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all content zones.
    /// </summary>
    Task<List<ContentZoneDTO>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates a new content zone.
    /// </summary>
    Task<ContentZoneDTO> CreateAsync(ContentZoneDTO zone, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing content zone.
    /// </summary>
    Task<bool> UpdateAsync(ContentZoneDTO zone, CancellationToken ct = default);

    /// <summary>
    /// Deletes a content zone and all its items.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Adds an item to a content zone.
    /// </summary>
    Task<ContentZoneItemDTO> AddItemAsync(Guid zoneId, ContentZoneItemDTO item, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing zone item.
    /// </summary>
    Task<bool> UpdateItemAsync(ContentZoneItemDTO item, CancellationToken ct = default);

    /// <summary>
    /// Removes an item from a content zone.
    /// </summary>
    Task<bool> RemoveItemAsync(Guid itemId, CancellationToken ct = default);

    /// <summary>
    /// Gets an item by ID.
    /// </summary>
    Task<ContentZoneItemDTO?> GetItemByIdAsync(Guid itemId, CancellationToken ct = default);

    /// <summary>
    /// Reorders items within a zone.
    /// </summary>
    Task<bool> ReorderItemsAsync(Guid zoneId, List<Guid> itemIdsInOrder, CancellationToken ct = default);
}
