using System.Threading;

namespace comjustinspicer.Data.Models.Blog;

public interface IPostService
{
    Task<List<PostDTO>> GetAllAsync(CancellationToken ct = default);
    Task<PostDTO?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PostDTO> CreateAsync(PostDTO post, CancellationToken ct = default);
    Task<bool> UpdateAsync(PostDTO post, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
