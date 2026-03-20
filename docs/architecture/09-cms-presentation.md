# Area 9: CMS View Components & Presentation

**Namespaces:**
- `Comjustinspicer.CMS.ViewComponents` (excluding `ContentZoneViewComponent`, covered in [Area 4](04-content-zone-framework.md))
- `Comjustinspicer.CMS.Views.*`
- `Comjustinspicer.CMS.Services` — `IViewDiscoveryService`, `ViewDiscoveryService`

**Depends on:** Content Domain Models (ViewModels), Content Zone Component Framework (admin zone edit views), Identity (admin views gated by `[Authorize]`)
**Consumed by:** Web project layout files via `CompiledRazorAssemblyPart`

---

## 1. Embedded Razor Views

The CMS library ships pre-compiled Razor views. This is enabled in `ServiceCollectionExtensions` by registering two application parts:

```csharp
apm.ApplicationParts.Add(new AssemblyPart(asm));           // controllers, ViewComponents
apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(asm)); // pre-compiled .cshtml views
```

The Web project uses runtime Razor compilation in development (`AddRazorRuntimeCompilation()`) so changes to `.cshtml` files in the Web project are picked up without rebuild. The CMS library's views are pre-compiled and are not affected by runtime compilation.

**View resolution precedence:** ASP.NET Core searches the Web project's `Views/` folder before falling back to the CMS library's compiled views. To override any CMS view, create a file at the same relative path in the Web project.

---

## 2. Admin Layout Structure

The CMS library provides the shared admin layout:

| File | Purpose |
|------|---------|
| `Views/Shared/_AdminLayout.cshtml` | Root admin layout with navigation, sidebar, Bulma CSS |
| `Views/Shared/_AdminNavbar.cshtml` | Top navigation bar (partial, included by `_AdminLayout`) |
| `Views/Shared/_ViewStart.cshtml` | Sets `_AdminLayout` as the default layout for admin views |
| `Views/AdminShared/VersionHistory.cshtml` | Shared version history list view used by all content types |
| `Views/AdminShared/_DeleteConfirmModal.cshtml` | Reusable delete confirmation modal partial |

Admin views specify the admin layout explicitly or inherit it via `_ViewStart.cshtml`.

---

## 3. Built-in View Components

### `PageViewComponent`

**Invocation:**
```razor
@await Component.InvokeAsync("Page", new { config = new PageContentZoneConfiguration() })
```

**Purpose:** Renders a page reference by fetching page data from `IPageModel`. Used to embed a page's content block zones as a widget within another zone.

**Parameter:** `PageContentZoneConfiguration? config` — contains the target page configuration (zone slot names).

---

### `ContentBlockViewComponent`

**Invocation:**
```razor
@await Component.InvokeAsync("ContentBlock", new { config = myConfig })
```

**Purpose:** Renders a reusable content block by ID. Fetches the latest published version from `IContentBlockModel` and renders it. Used as a widget within content zones.

**Parameter:** `ContentBlockContentZoneConfiguration config` — contains the content block identifier and rendering options.

---

### `ArticleViewComponent`

**Invocation:**
```razor
@await Component.InvokeAsync("Article", new { config = myConfig })
```

**Purpose:** Renders an article list or a specific article. Fetches from `IArticleListModel` and `IArticleModel`.

**Parameter:** `ArticleContentZoneConfiguration config` — contains the article list reference and display options (list view vs detail view).

---

### `LayoutViewComponent`

**Invocation:**
```razor
@await Component.InvokeAsync("Layout", new { config = new LayoutContentZoneConfiguration() })
```

**Purpose:** Renders a multi-column layout by composing multiple `ContentZone` components. The layout variant determines how columns are arranged.

**Parameter:** `LayoutContentZoneConfiguration config` — specifies the layout variant.

**Available layout variants:**

| Variant | Description |
|---------|-------------|
| `Default` | Single column (full width) |
| `SingleColumn` | Explicit single column |
| `TwoColumnEqual` | Two equal 50/50 columns |
| `TwoColumnSidebar` | Main content + narrow sidebar |
| `ThreeColumn` | Three equal columns |
| `FourColumn` | Four equal columns |
| `OneThirdTwoThird` | 1/3 + 2/3 split |
| `AsymmetricRightHeavy` | Narrow left + wide right |
| `CenteredNarrow` | Centered, constrained-width single column |
| `HeaderContentFooter` | Three stacked rows (header/body/footer) |
| `HeroWithColumns` | Full-width hero row + columned body |

Each variant renders named zone slots (`Column1`, `Column2`, `Header`, `Footer`, etc.) that editors populate with widgets.

---

## 4. Shared Admin Partials

Located in `Views/AdminShared/` (CMS library):

| Partial | Description |
|---------|-------------|
| `_DeleteConfirmModal.cshtml` | Bootstrap/Bulma modal for delete confirmation; renders form POST to the delete route |
| `VersionHistory.cshtml` | Full version list view with restore and delete-version actions |

Components directory (`Views/Shared/Components/`) contains the default views for each built-in ViewComponent (e.g., `ContentZone/Default.cshtml`, `ContentZone/Edit.cshtml`).

---

## 5. `IViewDiscoveryService`

`ViewDiscoveryService` scans the filesystem for `.cshtml` files (excluding partials prefixed with `_`) in standard ASP.NET view locations. It is a **scoped** service.

```csharp
public interface IViewDiscoveryService
{
    IReadOnlyList<string> GetAvailableViews(string componentName);
    IReadOnlyList<string> GetControllerViews(string controllerName);
}
```

**`GetAvailableViews(componentName)`** — scans:
- `{contentRoot}/Views/Shared/Components/{componentName}/`
- `{contentRoot}/Areas/*/Views/Shared/Components/{componentName}/`
- Sibling directories (to find views in `Comjustinspicer.CMS/Views/`)

Used by the `ViewPicker` `EditorType` — when an admin form has a `ViewPicker` field, the dropdown is populated with the discovered view names via the registry endpoint (`/admin/{contentType}/registry/{name}/properties`).

**`GetControllerViews(controllerName)`** — scans:
- `{contentRoot}/Views/{controllerName}/`
- Sibling directories

Used by `PageRegistryHandler.GetProperties` to return the list of available views for a page controller type, shown in the page-edit admin UI.

---

## 6. Overriding CMS Views in the Web Project

To replace any CMS view with a custom version:

1. Create a file at the same relative path in `Comjustinspicer.Web/Views/`
2. ASP.NET Core's view resolution searches the Web project first

For example, to replace the admin navbar:
- Create `Comjustinspicer.Web/Views/Shared/_AdminNavbar.cshtml`

For ViewComponent default views:
- Create `Comjustinspicer.Web/Views/Shared/Components/ContentBlock/Default.cshtml`

No configuration changes are needed; view resolution precedence handles the override automatically.
