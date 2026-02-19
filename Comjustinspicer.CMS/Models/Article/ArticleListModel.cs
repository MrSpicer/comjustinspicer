using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using AutoMapper;

namespace Comjustinspicer.CMS.Models.Article;

public sealed class ArticleListModel : IArticleListModel
{
    private readonly IContentService<ArticleDTO> _postService;
    private readonly IContentService<ArticleListDTO> _articleListService;
    private readonly IMapper _mapper;

    public ArticleListModel(IContentService<ArticleListDTO> articleListService, IContentService<ArticleDTO> postService, IMapper mapper)
    {
        _postService = postService ?? throw new ArgumentNullException(nameof(postService));
        _articleListService = articleListService ?? throw new ArgumentNullException(nameof(articleListService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ArticleListViewModel> GetIndexViewModelAsync(CancellationToken ct = default)
    {
        var vm = new ArticleListViewModel();
        var posts = await _postService.GetAllAsync(ct);
        vm.Articles = posts
            .Where(p => p.IsPublished && p.PublicationDate <= DateTime.UtcNow)
            .Select(p => _mapper.Map<ArticleViewModel>(p))
            .ToList();
        return vm;
    }

    public async Task<ArticleListIndexViewModel> GetArticleListIndexAsync(CancellationToken ct = default)
    {
        var lists = await _articleListService.GetAllAsync(ct);
        var posts = await _postService.GetAllAsync(ct);

        var vm = new ArticleListIndexViewModel
        {
            ArticleLists = lists.Select(l =>
            {
                var item = _mapper.Map<ArticleListItemViewModel>(l);
                item.ArticleCount = posts.Count(p => p.ArticleListMasterId == l.MasterId);
                return item;
            }).ToList()
        };
        return vm;
    }

    public async Task<ArticleListUpsertViewModel?> GetArticleListUpsertAsync(Guid? id, CancellationToken ct = default)
    {
        if (id == null) return new ArticleListUpsertViewModel();
        var dto = await _articleListService.GetByIdAsync(id.Value, ct);
        if (dto == null) return null;
        return _mapper.Map<ArticleListUpsertViewModel>(dto);
    }

    public async Task<(bool Success, string? ErrorMessage)> SaveArticleListUpsertAsync(ArticleListUpsertViewModel model, CancellationToken ct = default)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var dto = _mapper.Map<ArticleListDTO>(model);

        if (model.Id == null || model.Id == Guid.Empty)
        {
            await _articleListService.CreateAsync(dto, ct);
            return (true, null);
        }

        var success = await _articleListService.UpdateAsync(dto, ct);
        if (!success) return (false, "Unable to update article list. It may have been removed.");
        return (true, null);
    }

    public async Task<bool> DeleteArticleListAsync(Guid id, CancellationToken ct = default)
    {
        var list = await _articleListService.GetByIdAsync(id, ct);
        if (list == null) return false;
        var posts = await _postService.GetAllAsync(ct);
        foreach (var p in posts.Where(p => p.ArticleListMasterId == list.MasterId))
            await _postService.DeleteAsync(p.Id, false, true, ct);
        return await _articleListService.DeleteAsync(id, false, true, ct);
    }

    public async Task<ArticleListViewModel?> GetArticlesForListAsync(Guid articleListMasterId, CancellationToken ct = default)
    {
        var list = await _articleListService.GetByMasterIdAsync(articleListMasterId, ct);
        if (list == null) return null;

        var posts = await _postService.GetAllAsync(ct);
        return new ArticleListViewModel
        {
            ArticleListId = list.MasterId,
            ArticleListTitle = list.Title,
            ArticleListSlug = list.Slug,
            Articles = posts
                .Where(p => p.ArticleListMasterId == list.MasterId && p.IsPublished && p.PublicationDate <= DateTime.UtcNow)
                .Select(p => _mapper.Map<ArticleViewModel>(p))
                .ToList()
        };
    }

    public async Task<ArticleListViewModel?> GetArticlesForListBySlugAsync(string slug, CancellationToken ct = default)
    {
        var list = await _articleListService.GetBySlugAsync(slug, ct);
        if (list == null) return null;

        return await GetArticlesForListAsync(list.MasterId, ct);
    }
}
