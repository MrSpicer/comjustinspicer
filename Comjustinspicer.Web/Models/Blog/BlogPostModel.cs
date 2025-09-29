using comjustinspicer.Data.Models.Blog;

namespace comjustinspicer.Models.Blog;

public sealed class BlogPostModel : IBlogPostModel
{
    private readonly IPostService _postService;

    public BlogPostModel(IPostService postService)
    {
        _postService = postService ?? throw new ArgumentNullException(nameof(postService));
    }

    public async Task<PostViewModel?> GetPostViewModelAsync(Guid id, CancellationToken ct = default)
    {
        var dto = await _postService.GetByIdAsync(id, ct);
        if (dto == null) return null;
        return new PostViewModel(dto);
    }

    public async Task<PostUpsertViewModel?> GetUpsertViewModelAsync(Guid? id, CancellationToken ct = default)
    {
        if (id == null) return new PostUpsertViewModel();
        var dto = await _postService.GetByIdAsync(id.Value, ct);
        if (dto == null) return null;
        return PostUpsertViewModel.FromDto(dto);
    }

    public async Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(PostUpsertViewModel model, CancellationToken ct = default)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        var dto = model.ToDto();

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
