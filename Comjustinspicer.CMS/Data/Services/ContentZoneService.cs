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
            .Include(z => z.Items.Where(i =>
                i.IsActive &&
                !_context.ContentZoneItems.Any(i2 => i2.MasterId == i.MasterId && i2.Version > i.Version))
                .OrderBy(i => i.Ordinal))
            .AsNoTracking()
            .Where(z => z.Name == name && !z.IsDeleted && z.IsPublished
                && !_context.ContentZones.Any(z2 => z2.MasterId == z.MasterId && z2.Version > z.Version))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ContentZoneDTO?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ContentZones
            .Include(z => z.Items.Where(i =>
                i.IsActive &&
                !_context.ContentZoneItems.Any(i2 => i2.MasterId == i.MasterId && i2.Version > i.Version))
                .OrderBy(i => i.Ordinal))
            .AsNoTracking()
            .FirstOrDefaultAsync(z => z.Id == id, ct);
    }

    public async Task<List<ContentZoneDTO>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ContentZones
            .Include(z => z.Items.Where(i =>
                i.IsActive &&
                !_context.ContentZoneItems.Any(i2 => i2.MasterId == i.MasterId && i2.Version > i.Version))
                .OrderBy(i => i.Ordinal))
            .AsNoTracking()
            .Where(z => !z.IsDeleted
                && !_context.ContentZones.Any(z2 => z2.MasterId == z.MasterId && z2.Version > z.Version))
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

        zone.MasterId = zone.Id;
        _context.ContentZones.Add(zone);
        await _context.SaveChangesAsync(ct);
        return zone;
    }

    public async Task<bool> UpdateAsync(ContentZoneDTO zone, CancellationToken ct = default)
    {
        if (zone == null) throw new ArgumentNullException(nameof(zone));

        var existing = await _context.ContentZones.FirstOrDefaultAsync(z => z.Id == zone.Id, ct);
        if (existing == null) return false;

        var newVersion = existing with
        {
            Id = Guid.NewGuid(),
            Version = existing.Version + 1,
            Name = zone.Name,
            Title = zone.Title,
            Description = zone.Description,
            IsPublished = zone.IsPublished,
            IsArchived = zone.IsArchived,
            IsHidden = zone.IsHidden,
            ModificationDate = DateTime.UtcNow
        };
        _context.ContentZones.Add(newVersion);
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
        item.MasterId = item.Id;
        item.Version = 0;
        item.IsPublished = true;
        item.CreationDate = DateTime.UtcNow;
        item.ModificationDate = DateTime.UtcNow;

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

        var newVersion = existing with
        {
            Id = Guid.NewGuid(),
            Version = existing.Version + 1,
            ComponentName = item.ComponentName,
            ComponentPropertiesJson = item.ComponentPropertiesJson,
            IsActive = item.IsActive,
            ModificationDate = DateTime.UtcNow
            // Ordinal, ContentZoneId, MasterId preserved from existing
        };
        _context.ContentZoneItems.Add(newVersion);
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
            .Where(i => i.ContentZoneId == zoneId
                && !_context.ContentZoneItems.Any(i2 => i2.MasterId == i.MasterId && i2.Version > i.Version))
            .ToListAsync(ct);

        for (int i = 0; i < itemIdsInOrder.Count; i++)
        {
            var item = items.FirstOrDefault(x => x.Id == itemIdsInOrder[i]);
            if (item != null)
            {
                item.Ordinal = i + 1;
                item.ModificationDate = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ContentZoneAssignmentDTO?> GetByPageSlotAsync(Guid pageMasterId, string slotName, CancellationToken ct = default)
    {
        return await _context.ContentZoneAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ParentPageMasterId == pageMasterId && a.SlotName == slotName, ct);
    }

    public async Task<(ContentZoneDTO Zone, ContentZoneAssignmentDTO Assignment)> GetOrCreateByPageSlotAsync(Guid pageMasterId, string slotName, CancellationToken ct = default)
    {
        var assignment = await _context.ContentZoneAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ParentPageMasterId == pageMasterId && a.SlotName == slotName, ct);

        if (assignment != null)
        {
            var latestZone = await _context.ContentZones
                .Include(z => z.Items.Where(i =>
                    i.IsActive &&
                    !_context.ContentZoneItems.Any(i2 => i2.MasterId == i.MasterId && i2.Version > i.Version))
                    .OrderBy(i => i.Ordinal))
                .AsNoTracking()
                .Where(z => z.MasterId == assignment.ContentZoneId && !z.IsDeleted)
                .OrderByDescending(z => z.Version)
                .FirstOrDefaultAsync(ct);

            if (latestZone != null)
                return (latestZone, assignment);
        }

        // Create zone + assignment atomically
        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            // Re-check inside transaction to avoid duplicate creation
            assignment = await _context.ContentZoneAssignments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ParentPageMasterId == pageMasterId && a.SlotName == slotName, ct);

            if (assignment != null)
            {
                var latestZone = await _context.ContentZones
                    .Include(z => z.Items.Where(i =>
                        i.IsActive &&
                        !_context.ContentZoneItems.Any(i2 => i2.MasterId == i.MasterId && i2.Version > i.Version))
                        .OrderBy(i => i.Ordinal))
                    .Where(z => z.MasterId == assignment.ContentZoneId && !z.IsDeleted)
                    .OrderByDescending(z => z.Version)
                    .FirstOrDefaultAsync(ct);

                await transaction.RollbackAsync(ct);
                return (latestZone!, assignment);
            }

            var zoneId = Guid.NewGuid();
            var zone = new ContentZoneDTO
            {
                Id = zoneId,
                MasterId = zoneId,
                Name = slotName,
                Title = slotName,
                IsPublished = true,
                CreationDate = DateTime.UtcNow,
                ModificationDate = DateTime.UtcNow,
                PublicationDate = DateTime.UtcNow
            };
            _context.ContentZones.Add(zone);

            var newAssignment = new ContentZoneAssignmentDTO
            {
                Id = Guid.NewGuid(),
                SlotName = slotName,
                ContentZoneId = zoneId,
                ParentPageMasterId = pageMasterId,
                ParentZoneId = null
            };
            _context.ContentZoneAssignments.Add(newAssignment);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            newAssignment.ContentZone = zone;
            return (zone, newAssignment);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<ContentZoneAssignmentDTO?> GetByZoneSlotAsync(Guid parentZoneId, string slotName, CancellationToken ct = default)
    {
        return await _context.ContentZoneAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ParentZoneId == parentZoneId && a.SlotName == slotName, ct);
    }

    public async Task<(ContentZoneDTO Zone, ContentZoneAssignmentDTO Assignment)> GetOrCreateByZoneSlotAsync(Guid parentZoneId, string slotName, CancellationToken ct = default)
    {
        var assignment = await _context.ContentZoneAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ParentZoneId == parentZoneId && a.SlotName == slotName, ct);

        if (assignment != null)
        {
            var latestZone = await _context.ContentZones
                .Include(z => z.Items.Where(i =>
                    i.IsActive &&
                    !_context.ContentZoneItems.Any(i2 => i2.MasterId == i.MasterId && i2.Version > i.Version))
                    .OrderBy(i => i.Ordinal))
                .AsNoTracking()
                .Where(z => z.MasterId == assignment.ContentZoneId && !z.IsDeleted)
                .OrderByDescending(z => z.Version)
                .FirstOrDefaultAsync(ct);

            if (latestZone != null)
                return (latestZone, assignment);
        }

        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            assignment = await _context.ContentZoneAssignments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ParentZoneId == parentZoneId && a.SlotName == slotName, ct);

            if (assignment != null)
            {
                var latestZone = await _context.ContentZones
                    .Include(z => z.Items.Where(i =>
                        i.IsActive &&
                        !_context.ContentZoneItems.Any(i2 => i2.MasterId == i.MasterId && i2.Version > i.Version))
                        .OrderBy(i => i.Ordinal))
                    .Where(z => z.MasterId == assignment.ContentZoneId && !z.IsDeleted)
                    .OrderByDescending(z => z.Version)
                    .FirstOrDefaultAsync(ct);

                await transaction.RollbackAsync(ct);
                return (latestZone!, assignment);
            }

            var zoneId = Guid.NewGuid();
            var zone = new ContentZoneDTO
            {
                Id = zoneId,
                MasterId = zoneId,
                Name = slotName,
                Title = slotName,
                IsPublished = true,
                CreationDate = DateTime.UtcNow,
                ModificationDate = DateTime.UtcNow,
                PublicationDate = DateTime.UtcNow
            };
            _context.ContentZones.Add(zone);

            var newAssignment = new ContentZoneAssignmentDTO
            {
                Id = Guid.NewGuid(),
                SlotName = slotName,
                ContentZoneId = zoneId,
                ParentPageMasterId = null,
                ParentZoneId = parentZoneId
            };
            _context.ContentZoneAssignments.Add(newAssignment);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            newAssignment.ContentZone = zone;
            return (zone, newAssignment);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IEnumerable<ContentZoneAssignmentDTO>> GetAllAssignmentsForPageAsync(Guid pageMasterId, CancellationToken ct = default)
    {
        return await _context.ContentZoneAssignments
            .AsNoTracking()
            .Where(a => a.ParentPageMasterId == pageMasterId)
            .ToListAsync(ct);
    }

    public async Task<List<ContentZoneDTO>> GetAllByPageAsync(Guid pageMasterId, CancellationToken ct = default)
    {
        var assignments = await _context.ContentZoneAssignments
            .AsNoTracking()
            .Where(a => a.ParentPageMasterId == pageMasterId)
            .ToListAsync(ct);

        var masterIds = assignments.Select(a => a.ContentZoneId).ToList();

        return await _context.ContentZones
            .Include(z => z.Items.Where(i =>
                i.IsActive &&
                !_context.ContentZoneItems.Any(i2 => i2.MasterId == i.MasterId && i2.Version > i.Version))
                .OrderBy(i => i.Ordinal))
            .AsNoTracking()
            .Where(z => masterIds.Contains(z.MasterId) && !z.IsDeleted
                && !_context.ContentZones.Any(z2 => z2.MasterId == z.MasterId && z2.Version > z.Version))
            .OrderBy(z => z.Name)
            .ToListAsync(ct);
    }

    public async Task<List<ContentZoneDTO>> GetAllByParentZoneAsync(Guid parentZoneId, CancellationToken ct = default)
    {
        var assignments = await _context.ContentZoneAssignments
            .AsNoTracking()
            .Where(a => a.ParentZoneId == parentZoneId)
            .ToListAsync(ct);

        var masterIds = assignments.Select(a => a.ContentZoneId).ToList();

        return await _context.ContentZones
            .Include(z => z.Items.Where(i =>
                i.IsActive &&
                !_context.ContentZoneItems.Any(i2 => i2.MasterId == i.MasterId && i2.Version > i.Version))
                .OrderBy(i => i.Ordinal))
            .AsNoTracking()
            .Where(z => masterIds.Contains(z.MasterId) && !z.IsDeleted
                && !_context.ContentZones.Any(z2 => z2.MasterId == z.MasterId && z2.Version > z.Version))
            .OrderBy(z => z.Name)
            .ToListAsync(ct);
    }

    public async Task<ContentZoneDTO> GetOrCreateByNameAsync(string name, CancellationToken ct = default)
    {
        var zone = await _context.ContentZones
            .Include(z => z.Items.Where(i =>
                i.IsActive &&
                !_context.ContentZoneItems.Any(i2 => i2.MasterId == i.MasterId && i2.Version > i.Version))
                .OrderBy(i => i.Ordinal))
            .Where(z => z.Name == name && !z.IsDeleted && z.IsPublished
                && !_context.ContentZones.Any(z2 => z2.MasterId == z.MasterId && z2.Version > z.Version))
            .FirstOrDefaultAsync(ct);

        if (zone != null)
            return zone;

        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            zone = await _context.ContentZones
                .Include(z => z.Items.Where(i =>
                    i.IsActive &&
                    !_context.ContentZoneItems.Any(i2 => i2.MasterId == i.MasterId && i2.Version > i.Version))
                    .OrderBy(i => i.Ordinal))
                .Where(z => z.Name == name && !z.IsDeleted && z.IsPublished
                    && !_context.ContentZones.Any(z2 => z2.MasterId == z.MasterId && z2.Version > z.Version))
                .FirstOrDefaultAsync(ct);

            if (zone != null)
            {
                await transaction.RollbackAsync(ct);
                return zone;
            }

            var zoneId = Guid.NewGuid();
            zone = new ContentZoneDTO
            {
                Id = zoneId,
                MasterId = zoneId,
                Name = name,
                Title = name,
                IsPublished = true,
                CreationDate = DateTime.UtcNow,
                ModificationDate = DateTime.UtcNow,
                PublicationDate = DateTime.UtcNow
            };
            _context.ContentZones.Add(zone);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return zone;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<HashSet<Guid>> GetZoneIdsWithChildrenAsync(IEnumerable<Guid> zoneIds, CancellationToken ct = default)
    {
        var ids = zoneIds.ToList();
        if (ids.Count == 0)
            return [];

        var parentIds = await _context.ContentZoneAssignments
            .AsNoTracking()
            .Where(a => a.ParentZoneId != null && ids.Contains(a.ParentZoneId.Value))
            .Select(a => a.ParentZoneId!.Value)
            .Distinct()
            .ToListAsync(ct);

        return [.. parentIds];
    }

    public async Task<List<ContentZoneDTO>> GetAllVersionsAsync(Guid masterId, CancellationToken ct = default)
    {
        return await _context.ContentZones
            .AsNoTracking()
            .Where(z => z.MasterId == masterId)
            .OrderByDescending(z => z.Version)
            .ToListAsync(ct);
    }

    public async Task<List<ContentZoneItemDTO>> GetAllItemVersionsAsync(Guid itemMasterId, CancellationToken ct = default)
    {
        return await _context.ContentZoneItems
            .AsNoTracking()
            .Where(i => i.MasterId == itemMasterId)
            .OrderByDescending(i => i.Version)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, int>> GetAssignmentCountsByMasterIdAsync(IEnumerable<Guid> masterIds, CancellationToken ct = default)
    {
        var ids = masterIds.ToList();
        if (ids.Count == 0) return [];

        return await _context.ContentZoneAssignments
            .AsNoTracking()
            .Where(a => ids.Contains(a.ContentZoneId))
            .GroupBy(a => a.ContentZoneId)
            .Select(g => new { MasterId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MasterId, x => x.Count, ct);
    }
}
