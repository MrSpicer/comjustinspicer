using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Data.Services;

public interface IContentService<T> where T : BaseContentDTO
{
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T> CreateAsync(T entity, CancellationToken ct = default);
    Task<bool> UpdateAsync(T entity, CancellationToken ct = default);
    Task<bool> UpsertAsync(T entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
