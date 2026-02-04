using System.Threading;

namespace Comjustinspicer.CMS.Models.Article;

public interface IArticleListModel
{
    Task<ArticleListViewModel> GetIndexViewModelAsync(CancellationToken ct = default);
    Task<ArticleUpsertViewModel?> GetUpsertViewModelAsync(Guid? id, CancellationToken ct = default);
    Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(ArticleUpsertViewModel model, CancellationToken ct = default);
}
