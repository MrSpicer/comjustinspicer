using System.Threading;
using Comjustinspicer.Data.ContentBlock.Models;

namespace Comjustinspicer.Data.ContentBlock;

public interface IContentBlockService
{
	Task<List<ContentBlockDTO>> GetAllAsync(CancellationToken ct = default);
	Task<ContentBlockDTO?> GetByIdAsync(Guid id, CancellationToken ct = default);
	Task<bool> UpsertAsync(ContentBlockDTO contentBlock, CancellationToken ct = default);
	Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}