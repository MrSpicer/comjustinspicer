using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Data.Models;

public record ContentBlockDTO : BaseContentDTO
{
    public string Content { get; set; } = string.Empty;
}