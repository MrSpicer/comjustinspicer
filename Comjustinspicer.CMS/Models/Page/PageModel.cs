using AutoMapper;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using Comjustinspicer.CMS.Models.Shared;

namespace Comjustinspicer.CMS.Models.Page;

public sealed class PageModel : VersionedModel<PageDTO>, IPageModel
{
    private readonly IPageService _service;
    private readonly IMapper _mapper;

    protected override string VersionHistoryContentType => "pages";
    protected override string GetVersionHistoryBackUrl(string? parentKey = null) => "/admin/pages";
    protected override Task<List<PageDTO>> GetAllVersionsAsync(Guid masterId, CancellationToken ct) => _service.GetAllVersionsAsync(masterId, ct);
    protected override Task<bool> DeleteVersionCoreAsync(Guid id, CancellationToken ct) => _service.DeleteVersionAsync(id, ct);

    public PageModel(IPageService service, IMapper mapper)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PageDTO?> GetByRouteAsync(string route, CancellationToken ct = default)
    {
        return await _service.GetByRouteAsync(route, ct);
    }

    public async Task<PageIndexViewModel> GetPageIndexAsync(CancellationToken ct = default)
    {
        var pages = await _service.GetAllAsync(ct);
        return new PageIndexViewModel { Pages = BuildTree(pages) };
    }

    public async Task<PageUpsertViewModel?> GetPageUpsertAsync(Guid? id, CancellationToken ct = default)
    {
        if (id == null || id == Guid.Empty)
        {
            return new PageUpsertViewModel();
        }

        var dto = await _service.GetByIdAsync(id.Value, ct);
        if (dto == null)
        {
            return null;
        }

        return _mapper.Map<PageUpsertViewModel>(dto);
    }

    public async Task<(bool Success, string? ErrorMessage)> SavePageUpsertAsync(PageUpsertViewModel model, CancellationToken ct = default)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var dto = _mapper.Map<PageDTO>(model);

        if (model.Id.HasValue && model.Id != Guid.Empty)
        {
            var ok = await _service.UpdateAsync(dto, ct);
            if (!ok) return (false, "Failed to update page.");
        }
        else
        {
            await _service.CreateAsync(dto, ct);
        }

        return (true, null);
    }

    public async Task<bool> DeletePageAsync(Guid id, CancellationToken ct = default)
    {
        return await _service.DeleteAsync(id, ct);
    }

    public async Task<bool> IsRouteAvailableAsync(string route, Guid? excludeMasterId = null, CancellationToken ct = default)
    {
        return await _service.IsRouteAvailableAsync(route, excludeMasterId, ct);
    }

    public Task<VersionHistoryViewModel?> GetVersionHistoryAsync(Guid masterId, CancellationToken ct = default)
        => BuildVersionHistoryAsync(masterId, ct: ct);

    public async Task<PageUpsertViewModel?> GetPageUpsertForRestoreAsync(Guid historicalId, CancellationToken ct = default)
    {
        var historical = await _service.GetByIdAsync(historicalId, ct);
        if (historical == null) return null;
        var latest = await _service.GetAllVersionsAsync(historical.MasterId, ct);
        var latestVersion = latest.FirstOrDefault();
        if (latestVersion == null) return null;
        var vm = _mapper.Map<PageUpsertViewModel>(historical);
        vm.Id = latestVersion.Id;
        vm.Version = latestVersion.Version;
        return vm;
    }

    public Task<bool> DeletePageVersionAsync(Guid id, CancellationToken ct = default)
        => DeleteVersionCoreAsync(id, ct);

    private static List<PageTreeNode> BuildTree(List<PageDTO> pages)
    {
        var roots = new List<PageTreeNode>();
        var nodeMap = new Dictionary<string, PageTreeNode>(StringComparer.OrdinalIgnoreCase);

        var sortedPages = pages.OrderBy(p => p.Route).ToList();

        foreach (var page in sortedPages)
        {
            // Handle root "/" page directly — Trim('/').Split(...) produces no segments
            if (page.Route == "/")
            {
                if (!nodeMap.TryGetValue("/", out var rootNode))
                {
                    rootNode = new PageTreeNode
                    {
                        Route = "/",
                        Title = page.Title,
                        PageId = page.Id,
                        PageMasterId = page.MasterId,
                        PageVersion = page.Version,
                        ControllerName = page.ControllerName,
                        IsPublished = page.IsPublished
                    };
                    nodeMap["/"] = rootNode;
                    roots.Insert(0, rootNode);
                }
                else
                {
                    rootNode.Title = page.Title;
                    rootNode.PageId = page.Id;
                    rootNode.PageMasterId = page.MasterId;
                    rootNode.PageVersion = page.Version;
                    rootNode.ControllerName = page.ControllerName;
                    rootNode.IsPublished = page.IsPublished;
                }
                continue;
            }

            var segments = page.Route.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            var currentPath = "";

            for (int i = 0; i < segments.Length; i++)
            {
                currentPath = "/" + string.Join("/", segments.Take(i + 1));
                var isLeaf = i == segments.Length - 1;

                if (!nodeMap.TryGetValue(currentPath, out var node))
                {
                    node = new PageTreeNode
                    {
                        Route = currentPath,
                        Title = isLeaf ? page.Title : segments[i],
                        PageId = isLeaf ? page.Id : null,
                        PageMasterId = isLeaf ? page.MasterId : null,
                        PageVersion = isLeaf ? page.Version : 0,
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
                    node.Title = page.Title;
                    node.PageId = page.Id;
                    node.PageMasterId = page.MasterId;
                    node.PageVersion = page.Version;
                    node.ControllerName = page.ControllerName;
                    node.IsPublished = page.IsPublished;
                }
            }
        }

        return roots;
    }
}
