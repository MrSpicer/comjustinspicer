using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Models.Page;

/// <summary>
/// Model interface for page operations.
/// </summary>
public interface IPageModel
{
    Task<List<PageDTO>> GetAllAsync(CancellationToken ct = default);
    Task<PageDTO?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PageDTO?> GetByRouteAsync(string route, CancellationToken ct = default);
    Task<PageDTO> CreateAsync(PageDTO page, CancellationToken ct = default);
    Task<bool> UpdateAsync(PageDTO page, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<PageTreeNode>> GetRouteTreeAsync(CancellationToken ct = default);
}
