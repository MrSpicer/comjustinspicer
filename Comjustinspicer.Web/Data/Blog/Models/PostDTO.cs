using comjustinspicer.Data.Models;

namespace comjustinspicer.Data.Blog.Models;

public class PostDTO : BaseContentDTO
{
	public string Body { get; set; } = string.Empty;
	public string AuthorName { get; set; } = string.Empty;
}