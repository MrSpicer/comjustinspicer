using Comjustinspicer.Data.Blog.Models;
using Comjustinspicer.Data.Blog;
using AutoMapper;

namespace Comjustinspicer.Models.Blog;

public sealed class BlogModel : IBlogModel
{
    private readonly IPostService _postService;
    private readonly IMapper _mapper;

    public BlogModel(IPostService postService, IMapper mapper)
    {
        _postService = postService ?? throw new ArgumentNullException(nameof(postService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<BlogViewModel> GetIndexViewModelAsync(CancellationToken ct = default)
    {
        var vm = new BlogViewModel();
        var posts = await _postService.GetAllAsync(ct);
        vm.Posts = posts
            .Where(p => p.PublicationDate <= DateTime.UtcNow)
            .Select(p => _mapper.Map<PostViewModel>(p))
            .ToList();
        return vm;
    }

    public async Task<PostUpsertViewModel?> GetUpsertViewModelAsync(Guid? id, CancellationToken ct = default)
    {
        if (id == null) return new PostUpsertViewModel();

        var post = await _postService.GetByIdAsync(id.Value, ct);
        if (post == null) return null;

    return _mapper.Map<PostUpsertViewModel>(post);
    }

    public async Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(PostUpsertViewModel model, CancellationToken ct = default)
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
