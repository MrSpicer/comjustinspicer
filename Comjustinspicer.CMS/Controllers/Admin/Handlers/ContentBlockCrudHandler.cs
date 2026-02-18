using Microsoft.AspNetCore.Http;
using Comjustinspicer.CMS.Models.ContentBlock;

namespace Comjustinspicer.CMS.Controllers.Admin.Handlers;

public class ContentBlockCrudHandler : IAdminCrudHandler
{
    private readonly IContentBlockModel _model;

    public ContentBlockCrudHandler(IContentBlockModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public string ContentType => "contentblocks";
    public string DisplayName => "Content Block";
    public string[]? WriteRoles => null;

    public string IndexViewPath => "~/Views/AdminContentBlock/ContentBlocks.cshtml";
    public string UpsertViewPath => "~/Views/AdminContentBlock/ContentBlockUpsert.cshtml";

    public async Task<object> GetIndexViewModelAsync(CancellationToken ct = default)
        => await _model.GetContentBlockIndexAsync(ct);

    public async Task<object?> GetUpsertViewModelAsync(Guid? id, IQueryCollection query, CancellationToken ct = default)
        => await _model.GetUpsertModelAsync(id, ct);

    public object CreateEmptyUpsertViewModel() => new ContentBlockUpsertViewModel();

    public async Task<AdminSaveResult> SaveUpsertAsync(object model, CancellationToken ct = default)
    {
        var vm = (ContentBlockUpsertViewModel)model;
        var result = await _model.SaveUpsertAsync(vm, ct);
        return result.Success
            ? new AdminSaveResult(true)
            : new AdminSaveResult(false, result.ErrorMessage);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => _model.DeleteAsync(id, ct);

    public async Task<IEnumerable<object>> GetApiListAsync(CancellationToken ct = default)
    {
        var vm = await _model.GetContentBlockIndexAsync(ct);
        return vm.ContentBlocks.Select(cb => (object)new { id = cb.Id, title = cb.Title });
    }

    public bool HasSecondaryApiList => false;

    public Task<IEnumerable<object>> GetSecondaryApiListAsync(string key, CancellationToken ct = default)
        => Task.FromResult(Enumerable.Empty<object>());

    public IAdminRegistryHandler? RegistryHandler => null;
    public IAdminCrudChildHandler? ChildHandler => null;
}
