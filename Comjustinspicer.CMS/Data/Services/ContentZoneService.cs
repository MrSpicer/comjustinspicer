using Microsoft.EntityFrameworkCore;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.DbContexts;

namespace Comjustinspicer.CMS.Data.Services;

/// <summary>
/// Service for managing ContentZones and their items.
/// </summary>
public sealed class ContentZoneService : IContentZoneService
{
    private readonly ContentZoneContext _context;

    public ContentZoneService(ContentZoneContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ContentZoneDTO?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.ContentZones
            .Include(z => z.Items.Where(i => i.IsActive).OrderBy(i => i.Ordinal))
            .AsNoTracking()
            .FirstOrDefaultAsync(z => z.Name == name && !z.IsDeleted && z.IsPublished, ct);
    }

    public async Task<ContentZoneDTO?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ContentZones
            .Include(z => z.Items.OrderBy(i => i.Ordinal))
            .AsNoTracking()
            .FirstOrDefaultAsync(z => z.Id == id, ct);
    }

    public async Task<List<ContentZoneDTO>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ContentZones
            .Include(z => z.Items.OrderBy(i => i.Ordinal))
            .AsNoTracking()
            .Where(z => !z.IsDeleted)
            .OrderBy(z => z.Name)
            .ToListAsync(ct);
    }

    public async Task<ContentZoneDTO> CreateAsync(ContentZoneDTO zone, CancellationToken ct = default)
    {
        if (zone == null) throw new ArgumentNullException(nameof(zone));

        if (zone.Id == Guid.Empty)
            zone.Id = Guid.NewGuid();

        var now = DateTime.UtcNow;
        zone.CreationDate = now;
        zone.ModificationDate = now;
        if (zone.PublicationDate == default)
            zone.PublicationDate = now;

        _context.ContentZones.Add(zone);
        await _context.SaveChangesAsync(ct);
        return zone;
    }

    public async Task<bool> UpdateAsync(ContentZoneDTO zone, CancellationToken ct = default)
    {
        if (zone == null) throw new ArgumentNullException(nameof(zone));

        var existing = await _context.ContentZones.FirstOrDefaultAsync(z => z.Id == zone.Id, ct);
        if (existing == null) return false;

        zone.ModificationDate = DateTime.UtcNow;
        _context.Entry(existing).CurrentValues.SetValues(zone);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _context.ContentZones.FirstOrDefaultAsync(z => z.Id == id, ct);
        if (existing == null) return false;

        _context.ContentZones.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ContentZoneItemDTO> AddItemAsync(Guid zoneId, ContentZoneItemDTO item, CancellationToken ct = default)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        var zone = await _context.ContentZones.FirstOrDefaultAsync(z => z.Id == zoneId, ct);
        if (zone == null)
            throw new InvalidOperationException($"Content zone with ID {zoneId} not found.");

        if (item.Id == Guid.Empty)
            item.Id = Guid.NewGuid();

        item.ContentZoneId = zoneId;
        item.CreatedAt = DateTime.UtcNow;
        item.ModifiedAt = DateTime.UtcNow;

        // Auto-assign ordinal if not set
        if (item.Ordinal == 0)
        {
            var maxOrdinal = await _context.ContentZoneItems
                .Where(i => i.ContentZoneId == zoneId)
                .MaxAsync(i => (int?)i.Ordinal, ct) ?? 0;
            item.Ordinal = maxOrdinal + 1;
        }

        _context.ContentZoneItems.Add(item);
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task<bool> UpdateItemAsync(ContentZoneItemDTO item, CancellationToken ct = default)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        var existing = await _context.ContentZoneItems.FirstOrDefaultAsync(i => i.Id == item.Id, ct);
        if (existing == null) return false;

        item.ModifiedAt = DateTime.UtcNow;
        _context.Entry(existing).CurrentValues.SetValues(item);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RemoveItemAsync(Guid itemId, CancellationToken ct = default)
    {
        var existing = await _context.ContentZoneItems.FirstOrDefaultAsync(i => i.Id == itemId, ct);
        if (existing == null) return false;

        _context.ContentZoneItems.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ContentZoneItemDTO?> GetItemByIdAsync(Guid itemId, CancellationToken ct = default)
    {
        return await _context.ContentZoneItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == itemId, ct);
    }

    public async Task<bool> ReorderItemsAsync(Guid zoneId, List<Guid> itemIdsInOrder, CancellationToken ct = default)
    {
        var items = await _context.ContentZoneItems
            .Where(i => i.ContentZoneId == zoneId)
            .ToListAsync(ct);

        for (int i = 0; i < itemIdsInOrder.Count; i++)
        {
            var item = items.FirstOrDefault(x => x.Id == itemIdsInOrder[i]);
            if (item != null)
            {
                item.Ordinal = i + 1;
                item.ModifiedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(ct);
        return true;
    }
}
