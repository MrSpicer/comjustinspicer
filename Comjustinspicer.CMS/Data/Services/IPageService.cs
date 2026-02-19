using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Data.Services;

/// <summary>
/// Service interface for page-specific operations.
/// </summary>
public interface IPageService
{
    Task<List<PageDTO>> GetAllAsync(CancellationToken ct = default);
    Task<PageDTO?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PageDTO?> GetByRouteAsync(string route, CancellationToken ct = default);
    Task<PageDTO> CreateAsync(PageDTO page, CancellationToken ct = default);
    Task<bool> UpdateAsync(PageDTO page, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsRouteAvailableAsync(string route, Guid? excludeMasterId = null, CancellationToken ct = default);
}
