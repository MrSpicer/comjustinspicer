using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using Comjustinspicer.CMS.Models.Shared;
using AutoMapper;

namespace Comjustinspicer.CMS.Models.ContentBlock;

public sealed class ContentBlockModel : VersionedModel<ContentBlockDTO>, IContentBlockModel
{
    private readonly IContentService<ContentBlockDTO> _service;
    private readonly IMapper _mapper;

    protected override string VersionHistoryContentType => "contentblocks";
    protected override string GetVersionHistoryBackUrl(string? parentKey = null) => "/admin/contentblocks";
    protected override Task<List<ContentBlockDTO>> GetAllVersionsAsync(Guid masterId, CancellationToken ct) => _service.GetAllVersionsAsync(masterId, ct);
    protected override Task<bool> DeleteVersionCoreAsync(Guid id, CancellationToken ct) => _service.DeleteAsync(id, softDelete: false, deleteHistory: false, ct: ct);

    public ContentBlockModel(IContentService<ContentBlockDTO> service, IMapper mapper)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ContentBlockViewModel?> GetViewModelByMasterIdAsync(Guid masterId, CancellationToken ct = default)
    {
        var dto = await _service.GetByMasterIdAsync(masterId, ct);
        if (dto == null) return null;
        return _mapper.Map<ContentBlockViewModel>(dto);
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
        return await _service.DeleteAsync(id, false, true, ct);
    }

    public Task<VersionHistoryViewModel?> GetVersionHistoryAsync(Guid masterId, CancellationToken ct = default)
        => BuildVersionHistoryAsync(masterId, ct: ct);

    public async Task<ContentBlockUpsertViewModel?> GetUpsertModelForRestoreAsync(Guid historicalId, CancellationToken ct = default)
    {
        var historical = await _service.GetByIdAsync(historicalId, ct);
        if (historical == null) return null;
        var latest = await _service.GetByMasterIdAsync(historical.MasterId, ct);
        if (latest == null) return null;
        var vm = _mapper.Map<ContentBlockUpsertViewModel>(historical);
        vm.Id = latest.Id;
        vm.Version = latest.Version;
        return vm;
    }

    public Task<bool> DeleteVersionAsync(Guid id, CancellationToken ct = default)
        => DeleteVersionCoreAsync(id, ct);
}