using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Data.Models;

public record ArticleListDTO : BaseContentDTO
{
    public List<ArticleDTO> Articles { get; set; } = new();
}
