using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using AutoMapper;

namespace Comjustinspicer.CMS.Models.Article;

public sealed class ArticleModel : IArticleModel
{
    private readonly IContentService<PostDTO> _postService;
    private readonly IMapper _mapper;

    public ArticleModel(IContentService<PostDTO> postService, IMapper mapper)
    {
        _postService = postService ?? throw new ArgumentNullException(nameof(postService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ArticleViewModel?> GetPostViewModelAsync(Guid id, CancellationToken ct = default)
    {
        var dto = await _postService.GetByIdAsync(id, ct);
        if (dto == null) return null;
        return _mapper.Map<ArticleViewModel>(dto);
    }

    public async Task<ArticleViewModel?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var all = await _postService.GetAllAsync(ct);
        var dto = all.FirstOrDefault(p => string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));
        if (dto == null) return null;
        return _mapper.Map<ArticleViewModel>(dto);
    }

    public async Task<ArticleUpsertViewModel?> GetUpsertViewModelAsync(Guid? id, CancellationToken ct = default)
    {
        if (id == null) return new ArticleUpsertViewModel();
        var dto = await _postService.GetByIdAsync(id.Value, ct);
        if (dto == null) return null;
        return _mapper.Map<ArticleUpsertViewModel>(dto);
    }

    public async Task<ArticleUpsertViewModel?> GetUpsertViewModelAsync(Guid? id, Guid articleListId, CancellationToken ct = default)
    {
        if (id == null) return new ArticleUpsertViewModel { ArticleListId = articleListId };
        var dto = await _postService.GetByIdAsync(id.Value, ct);
        if (dto == null) return null;
        return _mapper.Map<ArticleUpsertViewModel>(dto);
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

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return await _postService.DeleteAsync(id, ct);
    }
}
