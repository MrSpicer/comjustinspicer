using System.Threading;

namespace Comjustinspicer.CMS.Models.Article;

public interface IArticleModel
{
    Task<ArticleViewModel?> GetPostViewModelAsync(Guid id, CancellationToken ct = default);
    Task<PostUpsertViewModel?> GetUpsertViewModelAsync(Guid? id, CancellationToken ct = default);
    Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(PostUpsertViewModel model, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
