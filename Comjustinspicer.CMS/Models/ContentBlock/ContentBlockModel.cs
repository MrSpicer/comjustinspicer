using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using AutoMapper;

namespace Comjustinspicer.CMS.Models.ContentBlock;

public sealed class ContentBlockModel : IContentBlockModel
{
    private readonly IContentService<ContentBlockDTO> _service;
    private readonly IMapper _mapper;

    public ContentBlockModel(IContentService<ContentBlockDTO> service, IMapper mapper)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ContentBlockDTO?> FromIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(id));

        return await _service.GetByIdAsync(id);
    }
    
    public async Task<ContentBlockIndexViewModel> GetContentBlockIndexAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        var items = dtos.Select(d => _mapper.Map<ContentBlockItemViewModel>(d)).ToList();
        return new ContentBlockIndexViewModel { ContentBlocks = items };
    }
    
    public async Task<ContentBlockUpsertViewModel?> GetUpsertModelAsync(Guid? id, CancellationToken ct = default)
    {
        if (id == null || id == Guid.Empty)
        {
            return new ContentBlockUpsertViewModel();
        }
        
        var dto = await _service.GetByIdAsync(id.Value, ct);
        if (dto == null)
        {
            return null;
        }
        
        return _mapper.Map<ContentBlockUpsertViewModel>(dto);
    }
    
    public async Task<(bool Success, string? ErrorMessage)> SaveUpsertAsync(ContentBlockUpsertViewModel model, CancellationToken ct = default)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));
        
        var dto = _mapper.Map<ContentBlockDTO>(model);
        var ok = await _service.UpsertAsync(dto, ct);
        if (!ok) return (false, "An error occurred while saving the content block.");
        return (true, null);
    }
    
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return await _service.DeleteAsync(id, ct);
    }
}