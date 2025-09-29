using System.Collections.Generic;

namespace comjustinspicer.Models.Blog;

public sealed class BlogViewModel
{
	public List<PostViewModel> Posts { get; set; } = new();

}