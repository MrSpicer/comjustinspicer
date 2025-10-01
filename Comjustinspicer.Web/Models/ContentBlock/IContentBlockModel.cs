using Comjustinspicer.Data.ContentBlock.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Comjustinspicer.Models.ContentBlock;

public interface IContentBlockModel
{
    Task<ContentBlockDTO?> FromIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ContentBlockDTO>> GetAllAsync(CancellationToken ct = default);
    Task<ContentBlockDTO?> GetUpsertModelAsync(Guid? id, CancellationToken ct = default);
    Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(ContentBlockDTO model, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
