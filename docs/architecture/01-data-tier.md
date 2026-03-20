# Area 1: Data Tier

**Namespaces:**
- `Comjustinspicer.CMS.Data.Models`
- `Comjustinspicer.CMS.Data.DbContexts`
- `Comjustinspicer.CMS.Data.Services`
- `Comjustinspicer.CMS.Migrations` (auto-generated, do not edit)

**Depends on:** PostgreSQL/Npgsql, EF Core 10, ASP.NET Identity (`ApplicationDbContext`)
**Consumed by:** Content Domain Models, Admin CRUD Framework, CMS Bootstrap, Tests

---

## 1. Why Five DbContexts on One Connection String

The CMS uses five separate EF Core `DbContext` classes, all connected to the same PostgreSQL database via `DefaultConnection`. Each context owns a distinct set of tables and maintains its own `__EFMigrationsHistory` table, allowing independent migration lifecycles.

| DbContext | Tables | Migration History Table |
|-----------|--------|------------------------|
| `ApplicationDbContext` | ASP.NET Identity tables | `__EFMigrationsHistory_Application` |
| `ArticleContext` | `Articles`, `ArticleLists` | `__EFMigrationsHistory_Article` |
| `ContentBlockContext` | `ContentBlocks` | `__EFMigrationsHistory_ContentBlock` |
| `ContentZoneContext` | `ContentZones`, `ContentZoneItems`, `ContentZoneAssignments` | `__EFMigrationsHistory_ContentZone` |
| `PageContext` | `Pages` | `__EFMigrationsHistory_Page` |

This aggregate-per-context pattern keeps content types independently evolvable. Adding a new content type only requires creating a new context and migration — existing contexts are unaffected.

---

## 2. `BaseContentDTO` — Fields and Semantics

All versioned content types inherit from `BaseContentDTO`:

```csharp
public abstract record BaseContentDTO
{
    public Guid Id { get; set; }          // Unique per row/version
    public Guid MasterId { get; set; }    // Stable identity across versions
    public int Version { get; set; }      // Monotonically increasing per MasterId

    public string Slug { get; set; }      // URL-friendly identifier; auto-generated from Title if empty
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
    public bool IsDeleted { get; set; }   // Soft-delete flag

    public Guid? ParentMasterId { get; set; }  // FK to parent's MasterId (child resources)

    public List<CustomField> CustomFields { get; set; } = new();  // JSONB flexible fields
}
```

**Versioning semantics:**
- `Id` is a new `Guid` on every save. Never reused.
- `MasterId` is set to the original `Id` on first insert, then remains constant. Use `MasterId` as the stable reference to a logical content item.
- `Version` starts at 0 and increments on every update. The row with the highest `Version` for a given `MasterId` is the current version.

**Publishing flags:**
- `IsPublished` — visible to public. When a new version is saved with `IsPublished = true`, all prior versions for the same `MasterId` have `IsPublished` set to `false`.
- `IsArchived` — content is retained but hidden from normal queries (application-level convention; not enforced by the service).
- `IsHidden` — content exists but should not appear in listings (application-level convention).
- `IsDeleted` — soft delete marker. `GetAllAsync` and `GetByRouteAsync` exclude soft-deleted records.

**`CustomFields`:** A `List<CustomField>` stored as JSONB. Provides a key-value extension point without schema migrations.

---

## 3. Built-in DTOs

### `PageDTO`
Extends `BaseContentDTO`. Key additional fields:
- `Route` — normalized URL path (lowercase, leading slash, no trailing slash), e.g. `/about/team`
- `ControllerName` — the `[PageController]`-decorated controller to dispatch to, e.g. `"GenericPage"`
- `ConfigurationJson` — JSON-serialized page config object (type determined by controller's `ConfigurationType`)
- `ViewName` — optional override for the Razor view name

### `ArticleListDTO`
Parent container for articles. Has no article-specific fields beyond `BaseContentDTO`.

### `ArticleDTO`
Extends `BaseContentDTO`. Uses `ParentMasterId` to reference the owning `ArticleListDTO.MasterId`.

### `ContentBlockDTO`
Extends `BaseContentDTO`. Stores reusable content blocks.

### `ContentZoneDTO`
Extends `BaseContentDTO`. Key fields:
- `Name` — slot identifier used for global lookup
- `Description`
- `Items` — navigation property: `List<ContentZoneItemDTO>` (loaded via EF Include)

### `ContentZoneItemDTO`
Not derived from `BaseContentDTO`. Versioned independently with its own `Id`/`MasterId`/`Version`. Key fields:
- `ContentZoneId` — FK to the owning `ContentZoneDTO.Id`
- `ComponentName` — name of the `[ContentZoneComponent]` ViewComponent to render
- `ComponentPropertiesJson` — JSON-serialized widget configuration
- `Ordinal` — display order within the zone
- `IsActive` — whether this item is currently visible

### `ContentZoneAssignmentDTO`
Join record scoping a zone to a page slot or a nested zone slot:
- `Id` — `Guid`
- `SlotName` — string slot name, e.g. `"Main"`, `"Sidebar"`
- `ContentZoneId` — references `ContentZoneDTO.MasterId`
- `ParentPageMasterId?` — set for page-scoped assignments
- `ParentZoneId?` — set for nested zone assignments; exactly one of these two is non-null

### `CustomField`
```csharp
public class CustomField
{
    public string Key { get; set; }
    public string Value { get; set; }
}
```

---

## 4. DbContext Catalog

**`ApplicationDbContext`** — inherits from `IdentityDbContext<IdentityUser>`. Owns Identity tables. Does not interact with content DTOs.

**`ArticleContext`** — owns `DbSet<ArticleDTO>` and `DbSet<ArticleListDTO>`. Both types share this context; `ContentService<ArticleDTO>` and `ContentService<ArticleListDTO>` are each scoped to it.

**`ContentBlockContext`** — owns `DbSet<ContentBlockDTO>`.

**`ContentZoneContext`** — owns `DbSet<ContentZoneDTO>`, `DbSet<ContentZoneItemDTO>`, `DbSet<ContentZoneAssignmentDTO>`. The Include navigation from zone → items is defined here.

**`PageContext`** — owns `DbSet<PageDTO>`.

---

## 5. `IContentService<T>` — Full Method Semantics

`ContentService<T>` is a sealed generic implementation registered once per content type. All queries use `AsNoTracking()` for reads. The "latest version only" query pattern is:

```csharp
.Where(e => !_set.Any(e2 => e2.MasterId == e.MasterId && e2.Version > e.Version))
```

This is an existence-based filter: exclude any row where a newer version (higher `Version`) exists for the same `MasterId`.

| Method | Behaviour |
|--------|-----------|
| `GetAllAsync` | Returns latest version of every non-soft-deleted item, ordered by `ModificationDate` descending |
| `GetByIdAsync(id)` | Returns the exact row with that `Id` (any version) |
| `GetByMasterIdAsync(masterId)` | Returns the latest version for the given `MasterId` |
| `GetAllVersionsAsync(masterId)` | Returns all versions for a `MasterId`, newest first |
| `GetBySlugAsync(slug)` | Returns the latest version with the given `Slug` |
| `GetChildrenAsync(parentMasterId)` | Returns latest versions of all items with `ParentMasterId = parentMasterId` |
| `GetRootsAsync()` | Returns latest versions of all items with `ParentMasterId = null` |
| `CreateAsync(entity)` | Sets `Id = Guid.NewGuid()`, then `MasterId = Id`; auto-generates `Slug` from `Title` if empty; sets timestamps; `Version` stays 0 |
| `UpdateAsync(entity)` | Verifies original `Id` exists; increments `Version`; assigns new `Id`; creates new row; clears `IsPublished` on prior versions if new version is published |
| `UpsertAsync(entity)` | Delegates to `CreateAsync` if `Id` or `MasterId` is empty; otherwise `UpdateAsync` |
| `DeleteAsync(id, softDelete, deleteHistory)` | See below |

**Delete modes:**
- `softDelete=false, deleteHistory=false` — hard-deletes the single row matching `id`
- `softDelete=true, deleteHistory=false` — sets `IsDeleted=true`, `IsPublished=false` on the matching row, then calls `UpdateAsync` (creates a new version recording the soft-delete)
- `deleteHistory=true, softDelete=false` — hard-deletes all versions for the same `MasterId`
- `deleteHistory=true, softDelete=true` — marks all versions for the same `MasterId` as deleted

---

## 6. `IPageService`

`PageService` wraps `PageContext` directly (not the generic `ContentService<T>`) because pages need route-specific logic.

| Method | Behaviour |
|--------|-----------|
| `GetAllAsync` | Latest non-deleted versions, ordered by `Route` |
| `GetByIdAsync(id)` | Exact row by `Id` |
| `GetByRouteAsync(route)` | Normalizes route → queries for published, non-deleted, latest version matching that route |
| `GetAllVersionsAsync(masterId)` | All versions newest-first |
| `CreateAsync(page)` | Sets `MasterId = Id`, `Version = 0`; normalizes route; sets timestamps |
| `UpdateAsync(page)` | Increments version; new row; clears prior `IsPublished` if new version is published |
| `DeleteAsync(id)` | Hard-deletes ALL versions for the `MasterId` |
| `DeleteVersionAsync(id)` | Deletes only the single version row matching `id` |
| `IsRouteAvailableAsync(route, excludeMasterId)` | Returns `true` if no published, non-deleted, latest-version page occupies that route; optionally excludes one `MasterId` (for edit-in-place checks) |

**Route normalization** (`NormalizeRoute`):
1. Trim and lowercase
2. Ensure leading `/`
3. Remove trailing `/` unless the route is exactly `/`

---

## 7. `IContentZoneService`

`ContentZoneService` wraps `ContentZoneContext`. Zones and their items are both versioned.

**Zone methods:**

| Method | Behaviour |
|--------|-----------|
| `GetByNameAsync(name)` | Returns the published, non-deleted, latest-version zone with that name, including active items sorted by `Ordinal` |
| `GetByIdAsync(id)` | Returns the zone with that exact `Id`, including items |
| `GetAllAsync` | All latest, non-deleted zones including items |
| `CreateAsync` | Sets `MasterId = Id`; sets timestamps; `Version = 0` |
| `UpdateAsync` | Creates a new version row using `record with { }` syntax; preserves `MasterId` |
| `DeleteAsync` | Hard-deletes the single zone row (not all versions) |

**Item methods:**

| Method | Behaviour |
|--------|-----------|
| `AddItemAsync(zoneId, item)` | Sets `ContentZoneId`, `MasterId = Id`, `Version = 0`, `IsPublished = true`; auto-assigns `Ordinal` as `maxOrdinal + 1` |
| `UpdateItemAsync(item)` | Creates a new version row; preserves `Ordinal`, `ContentZoneId`, `MasterId` |
| `RemoveItemAsync(itemId)` | Hard-deletes the single item row |
| `GetItemByIdAsync(itemId)` | Returns the exact item row |
| `ReorderItemsAsync(zoneId, itemIdsInOrder)` | Updates `Ordinal` on the latest-version items in place (does not create new versions) |

**Assignment and slot methods:**

| Method | Behaviour |
|--------|-----------|
| `GetByPageSlotAsync(pageMasterId, slotName)` | Returns the assignment for a page's named slot, or `null` |
| `GetOrCreateByPageSlotAsync(pageMasterId, slotName)` | Returns existing `(Zone, Assignment)` or creates both atomically in a transaction; double-checked locking inside the transaction prevents duplicate creation on concurrent first renders |
| `GetByZoneSlotAsync(parentZoneId, slotName)` | Returns the assignment for a parent zone's named slot |
| `GetOrCreateByZoneSlotAsync(parentZoneId, slotName)` | Same pattern for nested zone slots |
| `GetOrCreateByNameAsync(name)` | Returns or creates a global zone by name (no assignment); also transactional |
| `GetAllAssignmentsForPageAsync(pageMasterId)` | All assignments for a page |
| `GetAllByPageAsync(pageMasterId)` | All latest zones assigned to a page |
| `GetAllByParentZoneAsync(parentZoneId)` | All latest zones that are nested children of a zone |
| `GetZoneIdsWithChildrenAsync(zoneIds)` | Returns which of the provided zone IDs have at least one child zone |
| `GetAllVersionsAsync(masterId)` | All zone versions newest-first |
| `GetAllItemVersionsAsync(itemMasterId)` | All item versions newest-first |
| `GetAssignmentCountsByMasterIdAsync(masterIds)` | Assignment count per zone `MasterId` (used for admin "in use" indicators) |

---

## 8. How to Add a New Content Type's Data Layer

1. **Create a DTO** in `Comjustinspicer.CMS.Data/Data/Models/` extending `BaseContentDTO`:
   ```csharp
   public record MyThingDTO : BaseContentDTO
   {
       public string Body { get; set; } = string.Empty;
   }
   ```

2. **Create a DbContext** in `Comjustinspicer.CMS.Data/Data/DbContexts/`:
   ```csharp
   public class MyThingContext : DbContext
   {
       public DbSet<MyThingDTO> MyThings => Set<MyThingDTO>();
       public MyThingContext(DbContextOptions<MyThingContext> options) : base(options) { }
   }
   ```

3. **Add a migration** (from the repo root):
   ```bash
   dotnet ef migrations add InitialMyThing --context MyThingContext \
     --project Comjustinspicer.CMS.Data --startup-project Comjustinspicer.Web \
     --output-dir Migrations/MyThing
   ```

4. **Register in DI** (`Program.cs` or `ServiceCollectionExtensions.cs`):
   ```csharp
   services.AddDbContext<MyThingContext>(options =>
       options.UseNpgsql(connectionString,
           b => b.MigrationsHistoryTable("__EFMigrationsHistory_MyThing")));

   services.AddScoped<IContentService<MyThingDTO>>(sp =>
       new ContentService<MyThingDTO>(sp.GetRequiredService<MyThingContext>()));
   ```

Migrations are applied automatically at startup via `app.EnsureCMS()`.

---

*See also:* [docs/content-system.md](../content-system.md) for the full step-by-step content type creation guide including models, admin views, and mappings.
