using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Data.ContentBlock.Models;

public class ContentBlockDTO : BaseContentDTO
{
    public string Content { get; set; } = string.Empty;
}