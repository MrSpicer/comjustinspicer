using System.Threading;

namespace Comjustinspicer.CMS.Models.Article;

public interface IArticleListModel
{
    Task<ArticleListViewModel> GetIndexViewModelAsync(CancellationToken ct = default);
    Task<PostUpsertViewModel?> GetUpsertViewModelAsync(Guid? id, CancellationToken ct = default);
    Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(PostUpsertViewModel model, CancellationToken ct = default);
}
