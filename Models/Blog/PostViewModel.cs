using comjustinspicer.Data.Models.Blog;

namespace comjustinspicer.Models.Blog;

public sealed class PostViewModel
{
	public string Title { get; init; }

	public PostViewModel(Post post) => Title = post.Title;
}