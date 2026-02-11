using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.CMS.Attributes;
using Comjustinspicer.CMS.Models.Page;

namespace Comjustinspicer.CMS.ViewComponents;

[ContentZoneComponent(
    DisplayName = "Page Navigation",
    Description = "Renders navigation links for published CMS pages.",
    Category = "Navigation",
    ConfigurationType = typeof(PageContentZoneConfiguration),
    IconClass = "fa-sitemap",
    Order = 10
)]
public class PageViewComponent : ViewComponent
{
    private readonly IPageModel _model;

    public PageViewComponent(IPageModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public async Task<IViewComponentResult> InvokeAsync(PageContentZoneConfiguration? config = null)
    {
        var index = await _model.GetPageIndexAsync();
        var items = MapNodes(index.Pages);
        var viewName = config?.ViewName ?? "Default";
        return View(viewName, new PageNavigationViewModel { Items = items });
    }

    private static List<PageNavigationItem> MapNodes(List<PageTreeNode> nodes)
    {
        return nodes
            .Where(n => n.PageId.HasValue && n.IsPublished)
            .Select(n => new PageNavigationItem
            {
                Title = n.Title,
                Route = n.Route,
                IsPublished = n.IsPublished,
                Children = MapNodes(n.Children)
            })
            .ToList();
    }
}
