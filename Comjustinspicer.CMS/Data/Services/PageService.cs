using Microsoft.EntityFrameworkCore;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.DbContexts;

namespace Comjustinspicer.CMS.Data.Services;

/// <summary>
/// Service for managing dynamic pages.
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
            .Where(p => !p.IsDeleted)
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
            .FirstOrDefaultAsync(p => p.Route == route && !p.IsDeleted && p.IsPublished, ct);
    }

    public async Task<PageDTO> CreateAsync(PageDTO page, CancellationToken ct = default)
    {
        if (page == null) throw new ArgumentNullException(nameof(page));

        if (page.Id == Guid.Empty)
            page.Id = Guid.NewGuid();

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

        var existing = await _context.Pages.FirstOrDefaultAsync(p => p.Id == page.Id, ct);
        if (existing == null) return false;

        page.Route = NormalizeRoute(page.Route);
        page.ModificationDate = DateTime.UtcNow;
        _context.Entry(existing).CurrentValues.SetValues(page);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _context.Pages.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (existing == null) return false;

        existing.IsDeleted = true;
        existing.ModificationDate = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> IsRouteAvailableAsync(string route, Guid? excludeId = null, CancellationToken ct = default)
    {
        route = NormalizeRoute(route);

        var query = _context.Pages
            .Where(p => p.Route == route && !p.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

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
