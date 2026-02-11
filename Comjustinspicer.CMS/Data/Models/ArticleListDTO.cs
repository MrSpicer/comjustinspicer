using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Data.Models;

public class ArticleListDTO : BaseContentDTO
{
    public List<PostDTO> Articles { get; set; } = new();
}
