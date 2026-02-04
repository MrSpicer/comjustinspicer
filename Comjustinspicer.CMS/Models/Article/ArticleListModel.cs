using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using AutoMapper;

namespace Comjustinspicer.CMS.Models.Article;

public sealed class ArticleListModel : IArticleListModel
{
    private readonly IContentService<PostDTO> _postService;
    private readonly IContentService<ArticleListDTO> _ArticleService;
    private readonly IMapper _mapper;

    public ArticleListModel(IContentService<ArticleListDTO> ArticleService, IContentService<PostDTO> postService, IMapper mapper)
    {
        _postService = postService ?? throw new ArgumentNullException(nameof(postService));
        _ArticleService = ArticleService ?? throw new ArgumentNullException(nameof(ArticleService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ArticleListViewModel> GetIndexViewModelAsync(CancellationToken ct = default)
    {
        var vm = new ArticleListViewModel();
        var posts = await _postService.GetAllAsync(ct);
        vm.Articles = posts
            .Where(p => p.PublicationDate <= DateTime.UtcNow)
            .Select(p => _mapper.Map<ArticleViewModel>(p))
            .ToList();
        return vm;
    }

    public async Task<ArticleUpsertViewModel?> GetUpsertViewModelAsync(Guid? id, CancellationToken ct = default)
    {
        if (id == null) return new ArticleUpsertViewModel();

        var post = await _postService.GetByIdAsync(id.Value, ct);
        if (post == null) return null;

    return _mapper.Map<ArticleUpsertViewModel>(post);
    }

    public async Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(ArticleUpsertViewModel model, CancellationToken ct = default)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

    var dto = _mapper.Map<PostDTO>(model);

        if (model.Id == null || model.Id == Guid.Empty)
        {
            await _postService.CreateAsync(dto, ct);
            return (true, null);
        }

        var success = await _postService.UpdateAsync(dto, ct);
        if (!success) return (false, "Unable to update post. It may have been removed.");

        return (true, null);
    }
}
