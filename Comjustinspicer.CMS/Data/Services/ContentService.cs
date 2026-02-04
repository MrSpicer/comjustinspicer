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
            .OrderByDescending(e => e.Id)
            .ToListAsync(ct);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _set
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<T> CreateAsync(T entity, CancellationToken ct = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        if (entity.Id == Guid.Empty)
            entity.Id = Guid.NewGuid();

        // Auto-generate slug from title if slug is empty
        if (string.IsNullOrWhiteSpace(entity.Slug) && !string.IsNullOrWhiteSpace(entity.Title))
            entity.Slug = Uri.EscapeDataString(entity.Title);

        var now = DateTime.UtcNow;
        entity.CreationDate = now;
        entity.ModificationDate = now;
        if (entity.PublicationDate == default)
            entity.PublicationDate = now;

        _set.Add(entity);
        await _dbContext.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> UpdateAsync(T entity, CancellationToken ct = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var existing = await _set.FirstOrDefaultAsync(e => e.Id == entity.Id, ct);
        if (existing == null) return false;

        // Ensure modification timestamp reflects this update
        entity.ModificationDate = DateTime.UtcNow;
        // Copy all values from incoming entity onto the tracked entity
        _dbContext.Entry(existing).CurrentValues.SetValues(entity);

        _set.Update(existing);
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpsertAsync(T entity, CancellationToken ct = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        if (entity.Id == Guid.Empty)
        {
            await CreateAsync(entity, ct);
            return true;
        }

        var existing = await _set.FirstOrDefaultAsync(e => e.Id == entity.Id, ct);
        if (existing == null)
        {
            // treat as create
            // Auto-generate slug from title if slug is empty
            if (string.IsNullOrWhiteSpace(entity.Slug) && !string.IsNullOrWhiteSpace(entity.Title))
                entity.Slug = Uri.EscapeDataString(entity.Title);

            var now = DateTime.UtcNow;
            entity.CreationDate = now;
            entity.ModificationDate = now;
            if (entity.PublicationDate == default)
                entity.PublicationDate = now;

            _set.Add(entity);
        }
        else
        {
            entity.ModificationDate = DateTime.UtcNow;
            _dbContext.Entry(existing).CurrentValues.SetValues(entity);
            _set.Update(existing);
        }

        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _set.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (existing == null) return false;

        _set.Remove(existing);
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }
}
