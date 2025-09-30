using comjustinspicer.Data.ContentBlock;
using comjustinspicer.Data.ContentBlock.Models;

namespace comjustinspicer.Models.ContentBlock;

public class ContentBlockModel
{
    private IContentBlockService _service { get; set; }
    public ContentBlockModel(IContentBlockService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async Task<ContentBlockDTO> FromIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(id));

        return await _service.GetByIdAsync(id) ?? new ContentBlockDTO { Content = string.Empty };
    }
}