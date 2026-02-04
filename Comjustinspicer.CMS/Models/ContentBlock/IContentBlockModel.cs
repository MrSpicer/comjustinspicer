using Comjustinspicer.CMS.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Comjustinspicer.CMS.Models.ContentBlock;

public interface IContentBlockModel
{
    Task<ContentBlockDTO?> FromIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ContentBlockDTO>> GetAllAsync(CancellationToken ct = default);
    Task<ContentBlockUpsertViewModel?> GetUpsertModelAsync(Guid? id, CancellationToken ct = default);
    Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(ContentBlockUpsertViewModel model, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
