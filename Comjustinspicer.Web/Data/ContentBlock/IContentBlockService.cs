using System.Threading;

namespace comjustinspicer.Data.Models.ContentBlock;

public interface IContentBlockService
{
	Task<List<ContentBlockDTO>> GetAllAsync(CancellationToken ct = default);
	Task<ContentBlockDTO?> GetByIdAsync(Guid id, CancellationToken ct = default);
	Task<bool> UpsertAsync(ContentBlockDTO contentBlock, CancellationToken ct = default);
	Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}