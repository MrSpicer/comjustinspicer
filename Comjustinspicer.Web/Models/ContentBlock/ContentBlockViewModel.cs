using Comjustinspicer.Data.ContentBlock.Models;

namespace Comjustinspicer.Models.ContentBlock;

public sealed class ContentBlockViewModel
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Content { get; init; } = string.Empty;

}
