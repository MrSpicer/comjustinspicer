using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.CMS.Attributes;
using Comjustinspicer.CMS.Models.Article;

namespace Comjustinspicer.CMS.ViewComponents;

/// <summary>
/// Renders articles - either a list of articles or a single article by ID.
/// </summary>
[ContentZoneComponent(
    DisplayName = "Article",
    Description = "Displays blog articles - either a list or a single post.",
    Category = "Content",
    ConfigurationType = typeof(ArticleContentZoneConfiguration),
    IconClass = "fa-newspaper",
    Order = 2
)]
public class ArticleViewComponent : ViewComponent
{
    private readonly IArticleListModel _listModel;
    private readonly IArticleModel _articleModel;

    public ArticleViewComponent(IArticleListModel listModel, IArticleModel articleModel)
    {
        _listModel = listModel;
        _articleModel = articleModel;
    }

    public async Task<IViewComponentResult> InvokeAsync(ArticleContentZoneConfiguration config)
    {
        config ??= new ArticleContentZoneConfiguration();

        // If an ID is provided, load the article
        if (config.Id.HasValue && config.Id.Value != Guid.Empty)
        {
            var loadedArticle = await _articleModel.GetPostViewModelAsync(config.Id.Value);
            return View(config.ViewName ?? "Article", loadedArticle);
        }

        // If an article is passed in directly, render it
        if (config.Article != null)
        {
            return View(config.ViewName ?? "Article", config.Article);
        }

        // Render the list of articles
        var vm = await _listModel.GetIndexViewModelAsync(CancellationToken.None);
        return View(config.ViewName ?? "List", vm);
    }
}