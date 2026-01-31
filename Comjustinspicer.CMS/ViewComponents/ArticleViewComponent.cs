using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.CMS.Models.Article;

namespace Comjustinspicer.CMS.ViewComponents;

public class ArticleViewComponent : ViewComponent
{
    private readonly IArticleListModel _listModel;
    private readonly IArticleModel _articleModel;

    public ArticleViewComponent(IArticleListModel listModel, IArticleModel articleModel)
    {
        _listModel = listModel;
        _articleModel = articleModel;
    }

    public async Task<IViewComponentResult> InvokeAsync(string? viewName = null, ArticleViewModel? article = null, Guid? id = null)
    {
        // If an ID is provided, load the article
        if (id.HasValue)
        {
            var loadedArticle = await _articleModel.GetPostViewModelAsync(id.Value);
            return View(viewName ?? "Post", loadedArticle);
        }

        // If an article is passed in directly, render it
        if (article != null)
        {
            return View(viewName ?? "Post", article);
        }

        // Render the list of articles
        var vm = await _listModel.GetIndexViewModelAsync(CancellationToken.None);
        return View(viewName ?? "Default", vm);
    }
}