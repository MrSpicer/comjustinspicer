# Content System

The content system provides a generic, versioned approach to managing all CMS content types. Every content type shares a common base, a single generic service, and a unified admin CRUD framework.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [BaseContentDTO](#basecontentdto)
- [Built-in Content Types](#built-in-content-types)
- [Adding a New Content Type](#adding-a-new-content-type)

---

## Architecture Overview

```
BaseContentDTO (abstract record)
    ├── ContentBlockDTO
    ├── ArticleDTO
    ├── ArticleListDTO
    ├── PageDTO
    └── ContentZoneDTO

IContentService<T where T : BaseContentDTO>
    └── ContentService<T>  (single generic implementation)

VersionedModel<TDto>  (abstract)
    └── AdminCrudModel<TDto>  (abstract, also implements IAdminCrudHandler)
            ├── ContentBlockModel
            ├── ArticleListModel
            ├── PageModel
            └── ContentZoneModel
        ArticleModel  (extends VersionedModel<ArticleDTO> directly — child resource, no standalone admin handler)

IAdminCrudHandler  (interface)
    └── implemented by each AdminCrudModel subclass
    └── resolved via AdminHandlerRegistry
    └── driven by AdminContentController (single controller, all content types)
```

**Top-level vs child model types:**
- **Top-level** types extend `AdminCrudModel<TDto>`. They get their own admin list/edit UI and are registered as `IAdminCrudHandler` so `AdminHandlerRegistry` picks them up automatically.
- **Child** types (like `ArticleModel`) extend `VersionedModel<TDto>` directly and are managed through a parent model's inner child handler (`IAdminCrudChildHandler`). They do not register as `IAdminCrudHandler` and have no standalone admin UI.

When adding a new standalone content type, extend `AdminCrudModel<TDto>`.

---

## BaseContentDTO

**File:** `Comjustinspicer.CMS/Data/Models/BaseContentDTO.cs`

All content types are EF Core records that extend `BaseContentDTO`. It carries every field shared across all content types.

```csharp
public abstract record BaseContentDTO
{
    public Guid Id { get; set; }          // Primary key; new Guid per version
    public Guid MasterId { get; set; }    // Constant across all versions of one item
    public int Version { get; set; }      // Monotonically increasing; 0 on first save

    public string Slug { get; set; }      // URL segment; auto-derived from Title if blank
    public string Title { get; set; }

    public Guid CreatedBy { get; set; }
    public Guid LastModifiedBy { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime ModificationDate { get; set; }

    public DateTime PublicationDate { get; set; }
    public DateTime? PublicationEndDate { get; set; }

    public bool IsPublished { get; set; }
    public bool IsArchived { get; set; }
    public bool IsHidden { get; set; }
    public bool IsDeleted { get; set; }

    public List<CustomField> CustomFields { get; set; } = new();
}
```


## Built-in Content Types

| Content Type | ContentType | DTO | Model |
|---|---|---|---|
| Content Block | `contentblocks` | `ContentBlockDTO` | `ContentBlockModel` |
| Article List | `articlelists` | `ArticleListDTO` | `ArticleListModel` |
| Article (child) | child of `articlelists` | `ArticleDTO` | `ArticleModel` |
| Page | `pages` | `PageDTO` | `PageModel` |
| Content Zone | `contentzones` | `ContentZoneDTO` | `ContentZoneModel` |

### ContentBlock

Adds `string Content` (max 10,000 chars). Managed via a rich-text editor. Referenced elsewhere in views by MasterId.

### Article / ArticleList

`ArticleListDTO` is the parent container (its own versioned content type). `ArticleDTO` is a child and holds `ArticleListMasterId` as a FK. `ArticleListModel` exposes an inner `ArticleChildHandler` that implements `IAdminCrudChildHandler`.

### Page

Adds `string Route` (unique, must begin with `/`), `string ControllerName`, and `string ConfigurationJson` for per-page controller config. See `PageRouteTransformer` for how routes are resolved at request time.

### ContentZone

A named zone (`string Name`, `string Description`) that owns an ordered list of `ContentZoneItemDTO`. Each item stores `ComponentName` (a view component) and `ComponentPropertiesJson`. The `ContentZoneService` extends beyond `IContentService<T>` with zone-item management methods (`AddItemAsync`, `RemoveItemAsync`, `ReorderItemsAsync`).

---

## Adding a New Content Type

New content types belong in the **Web project** (`Comjustinspicer.Web`), not the CMS library. This keeps the CMS library stable while allowing the host application to define its own content.

Follow these steps to wire in a new content type that gets full versioning and admin CRUD for free.

### 1. Create the DTO

`Comjustinspicer.Web/Data/Models/MyContentDTO.cs`

```csharp
using Comjustinspicer.CMS.Data.Models;

namespace Comjustinspicer.Web.Data.Models;

public record MyContentDTO : BaseContentDTO
{
    public string Body { get; set; } = string.Empty;
}
```

### 2. Create the DbContext

`Comjustinspicer.Web/Data/DbContexts/MyContentContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Comjustinspicer.Web.Data.Models;

namespace Comjustinspicer.Web.Data.DbContexts;

public class MyContentContext : DbContext
{
    public MyContentContext(DbContextOptions<MyContentContext> options) : base(options) { }

    public DbSet<MyContentDTO> MyContents { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<MyContentDTO>(e =>
        {
            e.HasKey(e => e.Id);
            e.Property(e => e.Title).IsRequired();
            e.ToTable("MyContents");
            e.OwnsMany(e => e.CustomFields, cf => cf.ToJson());
        });
    }
}
```

### 3. Create a migration

```bash
dotnet ef migrations add AddMyContent \
  -s Comjustinspicer.Web/Comjustinspicer.Web.csproj \
  -p Comjustinspicer.Web/Comjustinspicer.Web.csproj \
  -c MyContentContext \
  -o Migrations/MyContent
```

Then apply:

```bash
dotnet ef database update \
  -s Comjustinspicer.Web/Comjustinspicer.Web.csproj \
  -c MyContentContext
```

### 4. Create ViewModels

`Comjustinspicer.Web/Models/MyContent/MyContentViewModel.cs`

```csharp
using Comjustinspicer.CMS.Models;

namespace Comjustinspicer.Web.Models.MyContent;

public class MyContentViewModel : BaseContentViewModel
{
    public string Body { get; set; } = string.Empty;
}
```

`Comjustinspicer.Web/Models/MyContent/MyContentUpsertViewModel.cs`

```csharp
using Comjustinspicer.CMS.Attributes;
using Comjustinspicer.CMS.Models;

namespace Comjustinspicer.Web.Models.MyContent;

public class MyContentUpsertViewModel : BaseContentViewModel
{
    [FormProperty(EditorType.RichText)]
    public string Body { get; set; } = string.Empty;
}
```

### 5. Add AutoMapper mappings

In `Comjustinspicer.Web/MappingProfile.cs`, add inside the constructor:

```csharp
// MyContent
CreateMap<MyContentDTO, MyContentViewModel>();
CreateMap<MyContentDTO, MyContentUpsertViewModel>();
CreateMap<MyContentUpsertViewModel, MyContentDTO>()
    .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id == Guid.Empty ? Guid.NewGuid() : s.Id))
    .ForMember(d => d.Slug, opt => opt.MapFrom(s =>
        string.IsNullOrWhiteSpace(s.Slug) ? Uri.EscapeDataString(s.Title) : s.Slug))
    .ForMember(d => d.CreatedBy, opt => opt.Ignore())
    .ForMember(d => d.LastModifiedBy, opt => opt.Ignore())
    .ForMember(d => d.CustomFields, opt => opt.Ignore());
```

### 6. Create the Model class

`Comjustinspicer.Web/Models/MyContent/MyContentModel.cs`

```csharp
using Microsoft.AspNetCore.Http;
using AutoMapper;
using Comjustinspicer.CMS.Controllers.Admin.Handlers;
using Comjustinspicer.CMS.Data.Services;
using Comjustinspicer.CMS.Models.Shared;
using Comjustinspicer.Web.Data.Models;

namespace Comjustinspicer.Web.Models.MyContent;

public sealed class MyContentModel : AdminCrudModel<MyContentDTO>
{
    private readonly IContentService<MyContentDTO> _service;
    private readonly IMapper _mapper;

    protected override string VersionHistoryContentType => "mycontents";
    protected override string GetVersionHistoryBackUrl(string? parentKey = null) => "/admin/mycontents";
    protected override Task<List<MyContentDTO>> GetAllVersionsAsync(Guid masterId, CancellationToken ct)
        => _service.GetAllVersionsAsync(masterId, ct);
    protected override Task<bool> DeleteVersionCoreAsync(Guid id, CancellationToken ct)
        => _service.DeleteAsync(id, softDelete: false, deleteHistory: false, ct: ct);

    public override string ContentType => "mycontents";
    public override string DisplayName => "My Content";
    public override string IndexViewPath => "~/Views/AdminMyContent/Index.cshtml";
    public override string UpsertViewPath => "~/Views/AdminMyContent/Upsert.cshtml";

    public MyContentModel(IContentService<MyContentDTO> service, IMapper mapper)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public override async Task<object> GetIndexViewModelAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(d => _mapper.Map<MyContentViewModel>(d)).ToList();
    }

    public override async Task<object?> GetUpsertViewModelAsync(Guid? id, IQueryCollection query, CancellationToken ct = default)
    {
        if (id == null || id == Guid.Empty)
            return new MyContentUpsertViewModel();

        var dto = await _service.GetByIdAsync(id.Value, ct);
        return dto == null ? null : _mapper.Map<MyContentUpsertViewModel>(dto);
    }

    public override object CreateEmptyUpsertViewModel() => new MyContentUpsertViewModel();

    public override async Task<AdminSaveResult> SaveUpsertAsync(object model, CancellationToken ct = default)
    {
        var vm = (MyContentUpsertViewModel)model;
        var dto = _mapper.Map<MyContentDTO>(vm);
        var ok = await _service.UpsertAsync(dto, ct);
        return ok ? new AdminSaveResult(true) : new AdminSaveResult(false, "Save failed.");
    }

    public override async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => await _service.DeleteAsync(id, softDelete: false, deleteHistory: true, ct: ct);

    public override async Task<IEnumerable<object>> GetApiListAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(d => (object)new { id = d.Id, title = d.Title });
    }
}
```

### 7. Create Razor views

`Comjustinspicer.Web/Views/AdminMyContent/Index.cshtml` — list all items using the standard admin table partial.

`Comjustinspicer.Web/Views/AdminMyContent/Upsert.cshtml` — the create/edit form. Use `@Html.EditorForModel()` or bind individual fields; the `[FormProperty]` attributes on the ViewModel drive dynamic form generation.

### 8. Register services

In `Comjustinspicer.Web/Program.cs`, before `builder.Services.AddComjustinspicerCms(...)`:

```csharp
// DbContext
builder.Services.AddDbContext<MyContentContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory_MyContent")));

// Generic content service
builder.Services.AddScoped<IContentService<MyContentDTO>>(sp =>
    new ContentService<MyContentDTO>(sp.GetRequiredService<MyContentContext>()));

// Model / handler
builder.Services.AddScoped<MyContentModel>();
builder.Services.AddScoped<IAdminCrudHandler>(sp => sp.GetRequiredService<MyContentModel>());
```

`AdminHandlerRegistry` picks up any `IAdminCrudHandler` registered in DI regardless of which project it originates from. `AdminContentController` handles all routes for `mycontents` with no additional controller code needed.

---
