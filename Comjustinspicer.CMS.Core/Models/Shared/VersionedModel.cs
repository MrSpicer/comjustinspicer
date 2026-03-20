using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.CMS.Models.Shared;

public abstract class VersionedModel<TDto> where TDto : BaseContentDTO
{
    protected abstract Task<List<TDto>> GetAllVersionsAsync(Guid masterId, CancellationToken ct);
    protected abstract Task<bool> DeleteVersionCoreAsync(Guid id, CancellationToken ct);
    protected abstract string VersionHistoryContentType { get; }
    protected abstract string GetVersionHistoryBackUrl(string? parentKey = null);

    protected async Task<VersionHistoryViewModel?> BuildVersionHistoryAsync(
        Guid masterId,
        string? parentKey = null,
        string? childType = null,
        CancellationToken ct = default)
    {
        var versions = await GetAllVersionsAsync(masterId, ct);
        if (!versions.Any()) return null;
        var maxVersion = versions.Max(v => v.Version);
        return new VersionHistoryViewModel
        {
            ContentType = VersionHistoryContentType,
            MasterId = masterId,
            ItemTitle = versions.First().Title ?? string.Empty,
            BackUrl = GetVersionHistoryBackUrl(parentKey),
            ParentKey = parentKey,
            ChildType = childType,
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
}
