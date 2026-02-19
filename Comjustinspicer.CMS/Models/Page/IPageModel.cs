using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Models.Page;

/// <summary>
/// Model interface for page operations.
/// </summary>
public interface IPageModel
{
    Task<PageDTO?> GetByRouteAsync(string route, CancellationToken ct = default);
    Task<PageIndexViewModel> GetPageIndexAsync(CancellationToken ct = default);
    Task<PageUpsertViewModel?> GetPageUpsertAsync(Guid? id, CancellationToken ct = default);
    Task<(bool Success, string? ErrorMessage)> SavePageUpsertAsync(PageUpsertViewModel model, CancellationToken ct = default);
    Task<bool> DeletePageAsync(Guid id, CancellationToken ct = default);
    Task<bool> IsRouteAvailableAsync(string route, Guid? excludeMasterId = null, CancellationToken ct = default);
}
