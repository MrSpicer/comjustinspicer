using Comjustinspicer.Data.ContentBlock.Models;

namespace Comjustinspicer.Models.ContentBlock;

public sealed class ContentBlockViewModel
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Content { get; init; } = string.Empty;

    public static ContentBlockViewModel FromDto(ContentBlockDTO? dto)
    {
        if (dto == null) return new ContentBlockViewModel { Content = string.Empty };

        // Presentation logic: ensure nulls become empty string, could sanitize here
        return new ContentBlockViewModel { Id = dto.Id, Content = dto.Content ?? string.Empty };
    }
}
