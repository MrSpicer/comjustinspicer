using System.Threading;

namespace comjustinspicer.Models.Blog;

public interface IBlogModel
{
    Task<BlogViewModel> GetIndexViewModelAsync(CancellationToken ct = default);
    Task<PostUpsertViewModel?> GetUpsertViewModelAsync(Guid? id, CancellationToken ct = default);
    Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(PostUpsertViewModel model, CancellationToken ct = default);
}
