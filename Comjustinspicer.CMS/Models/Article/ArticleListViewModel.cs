using System.Collections.Generic;

namespace Comjustinspicer.CMS.Models.Article;

public sealed class ArticleListViewModel
{
	public List<ArticleViewModel> Articles { get; set; } = new();

}