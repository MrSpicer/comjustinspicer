using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;

namespace Comjustinspicer.CMS.Models.ContentBlock;

public class ContentBlockModel : IContentBlockModel
{
    private IContentService<ContentBlockDTO> _service { get; set; }
    public ContentBlockModel(IContentService<ContentBlockDTO> service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async Task<ContentBlockDTO?> FromIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(id));

        return await _service.GetByIdAsync(id);
    }
    
    public async Task<List<ContentBlockDTO>> GetAllAsync(CancellationToken ct = default)
    {
        return await _service.GetAllAsync(ct);
    }
    
    public async Task<ContentBlockDTO?> GetUpsertModelAsync(Guid? id, CancellationToken ct = default)
    {
        if (id == null)
        {
            return new ContentBlockDTO();
        }
        
        var dto = await _service.GetByIdAsync(id.Value, ct);
        if (dto == null)
        {
            return new ContentBlockDTO();
        }
        
        return dto;
    }
    
    public async Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(ContentBlockDTO model, CancellationToken ct = default)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        
        var ok = await _service.UpsertAsync(model, ct);
        if (!ok) return (false, "An error occurred while saving the content block.");
        return (true, null);
    }
    
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return await _service.DeleteAsync(id, ct);
    }
}