using Comjustinspicer.CMS.Data.ContentBlock.Models;

namespace Comjustinspicer.CMS.Models.ContentBlock;

public sealed class ContentBlockViewModel
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Content { get; init; } = string.Empty;

}
