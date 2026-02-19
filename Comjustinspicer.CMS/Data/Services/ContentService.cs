using Microsoft.EntityFrameworkCore;
using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Data.Services;

public sealed class ContentService<T> : IContentService<T> where T : BaseContentDTO
{
    private readonly DbContext _dbContext;
    private readonly DbSet<T> _set;

    public ContentService(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _set = _dbContext.Set<T>();
    }

    public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _set
            .AsNoTracking()
            .Where(e => !_set.Any(e2 => e2.MasterId == e.MasterId && e2.Version > e.Version))
            .OrderByDescending(e => e.ModificationDate)
            .ToListAsync(ct);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _set
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<T?> GetByMasterIdAsync(Guid masterId, CancellationToken ct = default)
    {
        return await _set
            .AsNoTracking()
            .Where(e => e.MasterId == masterId)
            .OrderByDescending(e => e.Version)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<T> CreateAsync(T entity, CancellationToken ct = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        if (entity.Id == Guid.Empty)
            entity.Id = Guid.NewGuid();

        entity.MasterId = entity.Id; // set masterId to own id for initial version

        // Auto-generate slug from title if slug is empty
        if (string.IsNullOrWhiteSpace(entity.Slug) && !string.IsNullOrWhiteSpace(entity.Title))
            entity.Slug = Uri.EscapeDataString(entity.Title);

        var now = DateTime.UtcNow;
        entity.CreationDate = now;
        entity.ModificationDate = now;
        if (entity.IsPublished && entity.PublicationDate == default)
            entity.PublicationDate = now;

        _set.Add(entity);
        await _dbContext.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> UpdateAsync(T entity, CancellationToken ct = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var originalId = entity.Id;
        if (!await _set.AnyAsync(e => e.Id == originalId, ct))
            return false;

        entity.Version++;
        entity.Id = Guid.NewGuid(); // reset id for new version
        var now = DateTime.UtcNow;
        // Ensure modification timestamp reflects this update
        entity.ModificationDate = now;

        // Auto-generate slug from title if slug is empty
        if (string.IsNullOrWhiteSpace(entity.Slug) && !string.IsNullOrWhiteSpace(entity.Title))
            entity.Slug = Uri.EscapeDataString(entity.Title);

        if (entity.IsPublished && entity.PublicationDate == default)
            entity.PublicationDate = now;

        _dbContext.Add(entity);

        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpsertAsync(T entity, CancellationToken ct = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        if (entity.Id == Guid.Empty || entity.MasterId == Guid.Empty)
        {
            await CreateAsync(entity, ct);
            return true;
        }

       return await UpdateAsync(entity, ct);
    }

    public async Task<T?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        return await _set
            .AsNoTracking()
            .Where(e => e.Slug == slug
                     && !_set.Any(e2 => e2.MasterId == e.MasterId && e2.Version > e.Version))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, bool softDelete = false, bool deleteHistory = false, CancellationToken ct = default)
    {
        var entity = await _set.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity == null) return false;

        if (softDelete && !deleteHistory)
        {
            entity.IsDeleted = true;
            entity.IsPublished = false;
            return await UpdateAsync(entity, ct);
        }

        if(deleteHistory)
        {
            var historyItems = await _set
                .Where(e => e.MasterId == entity.MasterId)
                .ToListAsync(ct);

            if(softDelete)
            {
                foreach (var item in historyItems)
                {
                    item.IsDeleted = true;
                    item.IsPublished = false;
                }

                _dbContext.UpdateRange(historyItems);
                await _dbContext.SaveChangesAsync(ct);
                return true;
            }

            _set.RemoveRange(historyItems);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
        
        _set.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
}
