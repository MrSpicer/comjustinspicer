using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Comjustinspicer.CMS.Models.Article;

namespace Comjustinspicer.CMS.Controllers.Admin.Handlers;

public class ArticleCrudHandler : IAdminCrudHandler
{
    private readonly IArticleListModel _listModel;
    private readonly ArticleChildHandler _childHandler;

    public ArticleCrudHandler(IArticleListModel listModel, IArticleModel articleModel)
    {
        _listModel = listModel ?? throw new ArgumentNullException(nameof(listModel));
        _childHandler = new ArticleChildHandler(
            listModel ?? throw new ArgumentNullException(nameof(listModel)),
            articleModel ?? throw new ArgumentNullException(nameof(articleModel)));
    }

    public string ContentType => "articles";
    public string DisplayName => "Article List";
    public string[]? WriteRoles => null;

    public string IndexViewPath => "~/Views/AdminArticle/Index.cshtml";
    public string UpsertViewPath => "~/Views/AdminArticle/ArticleListUpsert.cshtml";

    public async Task<object> GetIndexViewModelAsync(CancellationToken ct = default)
        => await _listModel.GetArticleListIndexAsync(ct);

    public async Task<object?> GetUpsertViewModelAsync(Guid? id, IQueryCollection query, CancellationToken ct = default)
    {
        var vm = await _listModel.GetArticleListUpsertAsync(id, ct);
        if (vm == null && id != null) return null;
        return vm ?? new ArticleListUpsertViewModel();
    }

    public object CreateEmptyUpsertViewModel() => new ArticleListUpsertViewModel();

    public async Task<AdminSaveResult> SaveUpsertAsync(object model, CancellationToken ct = default)
    {
        var vm = (ArticleListUpsertViewModel)model;
        var result = await _listModel.SaveArticleListUpsertAsync(vm, ct);
        return result.Success
            ? new AdminSaveResult(true)
            : new AdminSaveResult(false, result.ErrorMessage);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => _listModel.DeleteArticleListAsync(id, ct);

    public async Task<IEnumerable<object>> GetApiListAsync(CancellationToken ct = default)
    {
        var vm = await _listModel.GetIndexViewModelAsync(ct);
        return vm.Articles.Select(p => (object)new { id = p.Id, title = p.Title });
    }

    public bool HasSecondaryApiList => true;

    public async Task<IEnumerable<object>> GetSecondaryApiListAsync(string key, CancellationToken ct = default)
    {
        if (!string.Equals(key, "articlelists", StringComparison.OrdinalIgnoreCase))
            return Enumerable.Empty<object>();

        var vm = await _listModel.GetArticleListIndexAsync(ct);
        return vm.ArticleLists.Select(l => (object)new { id = l.Id, title = l.Title });
    }

    public IAdminRegistryHandler? RegistryHandler => null;
    public IAdminCrudChildHandler? ChildHandler => _childHandler;
}

/// <summary>Manages articles within an article list (child entities).</summary>
internal sealed class ArticleChildHandler : IAdminCrudChildHandler
{
    private readonly IArticleListModel _listModel;
    private readonly IArticleModel _articleModel;

    public ArticleChildHandler(IArticleListModel listModel, IArticleModel articleModel)
    {
        _listModel = listModel;
        _articleModel = articleModel;
    }

    public string ChildType => "articles";
    public string ChildDisplayName => "Article";
    public string[]? WriteRoles => ["Admin", "Editor"];

    public string ChildIndexViewPath => "~/Views/AdminArticle/Articles.cshtml";
    public string ChildUpsertViewPath => "~/Views/AdminArticle/Upsert.cshtml";

    public async Task<object?> GetChildIndexViewModelAsync(string parentKey, CancellationToken ct = default)
        => await _listModel.GetArticlesForListBySlugAsync(parentKey, ct);

    public async Task<object?> GetChildUpsertViewModelAsync(string parentKey, Guid? id, CancellationToken ct = default)
    {
        var list = await _listModel.GetArticlesForListBySlugAsync(parentKey, ct);
        if (list == null) return null;
        var vm = await _articleModel.GetUpsertViewModelAsync(id, list.ArticleListId, ct);
        if (vm == null && id != null) return null;
        return vm;
    }

    public async Task SetChildUpsertViewDataAsync(ViewDataDictionary viewData, string parentKey, CancellationToken ct = default)
    {
        viewData["ArticleListSlug"] = parentKey;
        var list = await _listModel.GetArticlesForListBySlugAsync(parentKey, ct);
        viewData["ArticleListTitle"] = list?.ArticleListTitle;
    }

    public object CreateEmptyChildUpsertViewModel() => new ArticleUpsertViewModel();

    public async Task<AdminSaveResult> SaveChildUpsertAsync(string parentKey, object model, CancellationToken ct = default)
    {
        var vm = (ArticleUpsertViewModel)model;
        var result = await _articleModel.SaveUpsertAsync(vm, ct);
        return result.Success
            ? new AdminSaveResult(true)
            : new AdminSaveResult(false, result.ErrorMessage);
    }

    public Task<bool> DeleteChildAsync(Guid id, CancellationToken ct = default)
        => _articleModel.DeleteAsync(id, ct);

    public bool SupportsReorder => false;

    public Task<bool> ReorderAsync(string parentKey, List<Guid> orderedIds, CancellationToken ct = default)
        => Task.FromResult(false);
}
