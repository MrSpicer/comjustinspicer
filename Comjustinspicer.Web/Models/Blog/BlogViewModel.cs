using System.Collections.Generic;

namespace Comjustinspicer.Models.Blog;

public sealed class BlogViewModel
{
	public List<PostViewModel> Posts { get; set; } = new();

}