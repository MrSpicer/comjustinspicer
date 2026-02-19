using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Data.Services;
using Comjustinspicer.CMS.Models.Shared;
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

    public async Task<ContentBlockDTO?> FromMasterIdAsync(Guid masterId, CancellationToken ct = default)
    {
        if (masterId == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(masterId));
        return await _service.GetByMasterIdAsync(masterId, ct);
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

    public async Task<VersionHistoryViewModel?> GetVersionHistoryAsync(Guid masterId, CancellationToken ct = default)
    {
        var versions = await _service.GetAllVersionsAsync(masterId, ct);
        if (!versions.Any()) return null;
        var maxVersion = versions.Max(v => v.Version);
        return new VersionHistoryViewModel
        {
            ContentType = "contentblocks",
            MasterId = masterId,
            ItemTitle = versions.First().Title ?? string.Empty,
            BackUrl = "/admin/contentblocks",
            Versions = versions.Select(v => new VersionItemViewModel
            {
                Id = v.Id,
                Version = v.Version,
                Title = v.Title ?? string.Empty,
                CreationDate = v.CreationDate,
                ModificationDate = v.ModificationDate,
                IsPublished = v.IsPublished,
                IsDeleted = v.IsDeleted,
                IsLatest = v.Version == maxVersion
            }).ToList()
        };
    }

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
        => _service.DeleteAsync(id, softDelete: false, deleteHistory: false, ct: ct);
}