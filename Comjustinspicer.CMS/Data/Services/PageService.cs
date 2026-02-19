using Microsoft.EntityFrameworkCore;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.DbContexts;

namespace Comjustinspicer.CMS.Data.Services;

/// <summary>
/// Service for managing dynamic pages with versioning support.
/// </summary>
public sealed class PageService : IPageService
{
    private readonly PageContext _context;

    public PageService(PageContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<PageDTO>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Pages
            .AsNoTracking()
            .Where(p => !p.IsDeleted
                && !_context.Pages.Any(p2 => p2.MasterId == p.MasterId && p2.Version > p.Version))
            .OrderBy(p => p.Route)
            .ToListAsync(ct);
    }

    public async Task<PageDTO?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Pages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<PageDTO?> GetByRouteAsync(string route, CancellationToken ct = default)
    {
        route = NormalizeRoute(route);

        return await _context.Pages
            .AsNoTracking()
            .Where(p => p.Route == route && !p.IsDeleted && p.IsPublished
                && !_context.Pages.Any(p2 => p2.MasterId == p.MasterId && p2.Version > p.Version))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<PageDTO>> GetAllVersionsAsync(Guid masterId, CancellationToken ct = default)
        => await _context.Pages
            .AsNoTracking()
            .Where(p => p.MasterId == masterId)
            .OrderByDescending(p => p.Version)
            .ToListAsync(ct);

    public async Task<PageDTO> CreateAsync(PageDTO page, CancellationToken ct = default)
    {
        if (page == null) throw new ArgumentNullException(nameof(page));

        if (page.Id == Guid.Empty)
            page.Id = Guid.NewGuid();

        page.MasterId = page.Id;
        page.Version = 0;
        page.Route = NormalizeRoute(page.Route);

        var now = DateTime.UtcNow;
        page.CreationDate = now;
        page.ModificationDate = now;
        if (page.PublicationDate == default)
            page.PublicationDate = now;

        _context.Pages.Add(page);
        await _context.SaveChangesAsync(ct);
        return page;
    }

    public async Task<bool> UpdateAsync(PageDTO page, CancellationToken ct = default)
    {
        if (page == null) throw new ArgumentNullException(nameof(page));

        if (!await _context.Pages.AnyAsync(p => p.Id == page.Id, ct))
            return false;

        page.Version++;
        page.Id = Guid.NewGuid();
        page.Route = NormalizeRoute(page.Route);
        page.ModificationDate = DateTime.UtcNow;
        if (page.IsPublished && page.PublicationDate == default)
            page.PublicationDate = DateTime.UtcNow;

        _context.Pages.Add(page);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Pages.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity == null) return false;

        var allVersions = await _context.Pages
            .Where(p => p.MasterId == entity.MasterId)
            .ToListAsync(ct);

        _context.Pages.RemoveRange(allVersions);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteVersionAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Pages.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (entity == null) return false;
        _context.Pages.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> IsRouteAvailableAsync(string route, Guid? excludeMasterId = null, CancellationToken ct = default)
    {
        route = NormalizeRoute(route);

        var query = _context.Pages
            .Where(p => p.Route == route
                && !p.IsDeleted
                && !_context.Pages.Any(p2 => p2.MasterId == p.MasterId && p2.Version > p.Version));

        if (excludeMasterId.HasValue)
            query = query.Where(p => p.MasterId != excludeMasterId.Value);

        return !await query.AnyAsync(ct);
    }

    private static string NormalizeRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            return "/";

        route = route.Trim().ToLowerInvariant();

        // Ensure leading slash
        if (!route.StartsWith('/'))
            route = "/" + route;

        // Remove trailing slash (but keep root "/")
        if (route.Length > 1 && route.EndsWith('/'))
            route = route.TrimEnd('/');

        return route;
    }
}
