using System.Threading;

namespace Comjustinspicer.Models.Blog;

public interface IBlogPostModel
{
    Task<PostViewModel?> GetPostViewModelAsync(Guid id, CancellationToken ct = default);
    Task<PostUpsertViewModel?> GetUpsertViewModelAsync(Guid? id, CancellationToken ct = default);
    Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(PostUpsertViewModel model, CancellationToken ct = default);
}
