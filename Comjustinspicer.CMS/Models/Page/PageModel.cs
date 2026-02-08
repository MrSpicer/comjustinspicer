using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using Comjustinspicer.CMS.Pages;

namespace Comjustinspicer.CMS.Models.Page;

public class PageModel : IPageModel
{
    private readonly IPageService _service;
    private readonly IPageControllerRegistry _registry;

    public PageModel(IPageService service, IPageControllerRegistry registry)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public async Task<List<PageDTO>> GetAllAsync(CancellationToken ct = default)
    {
        return await _service.GetAllAsync(ct);
    }

    public async Task<PageDTO?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _service.GetByIdAsync(id, ct);
    }

    public async Task<PageDTO?> GetByRouteAsync(string route, CancellationToken ct = default)
    {
        return await _service.GetByRouteAsync(route, ct);
    }

    public async Task<PageDTO> CreateAsync(PageDTO page, CancellationToken ct = default)
    {
        return await _service.CreateAsync(page, ct);
    }

    public async Task<bool> UpdateAsync(PageDTO page, CancellationToken ct = default)
    {
        return await _service.UpdateAsync(page, ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return await _service.DeleteAsync(id, ct);
    }

    public async Task<List<PageTreeNode>> GetRouteTreeAsync(CancellationToken ct = default)
    {
        var pages = await _service.GetAllAsync(ct);
        return BuildTree(pages);
    }

    private static List<PageTreeNode> BuildTree(List<PageDTO> pages)
    {
        var roots = new List<PageTreeNode>();
        // Map route → node for quick parent lookup
        var nodeMap = new Dictionary<string, PageTreeNode>(StringComparer.OrdinalIgnoreCase);

        // Sort by route so parents are processed before children
        var sortedPages = pages.OrderBy(p => p.Route).ToList();

        foreach (var page in sortedPages)
        {
            var segments = page.Route.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            var currentPath = "";

            for (int i = 0; i < segments.Length; i++)
            {
                currentPath = "/" + string.Join("/", segments.Take(i + 1));
                var isLeaf = i == segments.Length - 1;

                if (!nodeMap.TryGetValue(currentPath, out var node))
                {
                    // Create intermediate or leaf node
                    node = new PageTreeNode
                    {
                        Route = currentPath,
                        Title = isLeaf ? page.Title : segments[i],
                        PageId = isLeaf ? page.Id : null,
                        ControllerName = isLeaf ? page.ControllerName : string.Empty,
                        IsPublished = isLeaf && page.IsPublished
                    };
                    nodeMap[currentPath] = node;

                    if (i == 0)
                    {
                        roots.Add(node);
                    }
                    else
                    {
                        var parentPath = "/" + string.Join("/", segments.Take(i));
                        if (nodeMap.TryGetValue(parentPath, out var parentNode))
                        {
                            parentNode.Children.Add(node);
                        }
                    }
                }
                else if (isLeaf)
                {
                    // Node exists as intermediate — upgrade to real page
                    node.Title = page.Title;
                    node.PageId = page.Id;
                    node.ControllerName = page.ControllerName;
                    node.IsPublished = page.IsPublished;
                }
            }
        }

        return roots;
    }
}
