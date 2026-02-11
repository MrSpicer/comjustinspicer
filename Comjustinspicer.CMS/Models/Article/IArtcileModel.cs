namespace Comjustinspicer.CMS.Models.Article;

public interface IArticleModel
{
    Task<ArticleViewModel?> GetPostViewModelAsync(Guid id, CancellationToken ct = default);
    Task<ArticleViewModel?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<ArticleUpsertViewModel?> GetUpsertViewModelAsync(Guid? id, CancellationToken ct = default);
    Task<ArticleUpsertViewModel?> GetUpsertViewModelAsync(Guid? id, Guid articleListId, CancellationToken ct = default);
    Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(ArticleUpsertViewModel model, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
