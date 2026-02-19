namespace Comjustinspicer.CMS.Models.Article;

public interface IArticleListModel
{
    Task<ArticleListViewModel> GetIndexViewModelAsync(CancellationToken ct = default);
    Task<ArticleListIndexViewModel> GetArticleListIndexAsync(CancellationToken ct = default);
    Task<ArticleListUpsertViewModel?> GetArticleListUpsertAsync(Guid? id, CancellationToken ct = default);
    Task<(bool Success, string? ErrorMessage)> SaveArticleListUpsertAsync(ArticleListUpsertViewModel model, CancellationToken ct = default);
    Task<bool> DeleteArticleListAsync(Guid id, CancellationToken ct = default);
    Task<ArticleListViewModel?> GetArticlesForListAsync(Guid articleListMasterId, CancellationToken ct = default);
    Task<ArticleListViewModel?> GetArticlesForListBySlugAsync(string slug, CancellationToken ct = default);
}
