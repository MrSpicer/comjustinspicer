# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Dev Server

- **URL:** `https://localhost:7046/`

## Commands

- **Build:** `dotnet build`
- **Run dev (hot reload + Sass watch):** `./Scripts/HotReloadRun.sh`
- **Run tests:** `./Scripts/TestsRun.sh` (loads AutoMapper license from user-secrets)
- **Run single test:** `dotnet test --filter "Name~MethodName"`
- **Apply migrations:** `./Scripts/ApplyMigrations.sh`
- **Rebuild Ef Migrations (destructive):** `./Scripts/RebuildEFMigrations.sh`
- **Docker build:** `./Scripts/DockerBuild.sh`

## Architecture

This is an ASP.NET Core MVC CMS with two main projects:

- **Comjustinspicer.Web** - Host application. Contains `Program.cs` (entry point), site-level views/assets, and `ErrorController`. Minimal code; delegates to CMS library.
- **Comjustinspicer.CMS** - Reusable class library providing all CMS functionality: controllers, models, services, data layer, view components, Identity scaffolding, and routing.
- **Comjustinspicer.Tests** - NUnit tests for the CMS library.

### Dynamic Page Routing

Pages are database-driven. `PageRouteTransformer` (a `DynamicRouteValueTransformer`) intercepts all requests via `MapDynamicControllerRoute<PageRouteTransformer>("{**slug}")`, looks up the route in the database, and dispatches to the appropriate page controller. Page data and config are passed via `HttpContext.Items["CMS:PageData"]` and `["CMS:PageConfig"]`.

Page controllers extend `PageControllerBase<TConfig>` and are discovered at startup via `[PageController]` attribute + `PageControllerRegistry` assembly scanning. The `GenericPageController` is the default page type.

### Content System

- **ContentZones** - Composable regions within pages. Components register via `[ContentZoneComponent]` attribute and are discovered by `ContentZoneComponentRegistry`.
- **ContentBlocks** - Standalone HTML content blocks.
- **Articles** - Blog-style posts managed through `ArticleModel`/`ArticleListModel`.

### Data Layer

Uses multiple EF Core DbContexts sharing a single PostgreSQL database, each with its own migrations history table:
- `ApplicationDbContext` (Identity), `ArticleContext`, `ContentBlockContext`, `ContentZoneContext`, `PageContext`

Content access uses a generic `IContentService<T>` / `ContentService<T>` pattern. Business logic lives in Model classes (e.g., `PageModel`, `ContentBlockModel`, `ArticleModel`) that sit between controllers and services.

### CMS Bootstrap

`app.EnsureCMS()` in `Program.cs` handles startup: applies pending migrations, seeds roles (Admin/Editor/User) and admin user, creates a default home page at `/`, and configures middleware. Seeding steps can be individually skipped via environment variables (`COMJUSTINSPICER_SKIP_MIGRATIONS`, `COMJUSTINSPICER_SKIP_ROLESEED`, `COMJUSTINSPICER_SKIP_DEFAULTPAGE`).

### Registration

All CMS services are registered via `services.AddComjustinspicerCms(configuration)` in `ServiceCollectionExtensions.cs`. This configures all DbContexts, Identity, services, model classes, registries, and adds the CMS assembly as an MVC application part.

## Code Conventions

- File-scoped namespaces, nullable reference types enabled
- Private fields: `_camelCase`; async methods: suffix `Async`
- ViewModels: `{Name}ViewModel.cs`; DTOs: `{Name}DTO.cs` (in `Data/Models/`)
- Constructor injection with `?? throw new ArgumentNullException(nameof(...))`
- Fallible operations return `(bool Success, string? ErrorMessage)` tuples
- Async methods include `CancellationToken ct = default`
- Logging: `Serilog.Log.ForContext<ClassName>()`
- Controller routing: attribute-based with `[Authorize]`, `[ValidateAntiForgeryToken]`
- Test naming: `MethodName_Scenario_ExpectedBehavior`, NUnit constraint model (`Assert.That(...)`)
- Import order: System > Microsoft > Third-party > Project
- Configuration form fields use `[FormProperty]` attribute with `EditorType` enum

## Secrets

Managed via `dotnet user-secrets` (project: `Comjustinspicer.Web`):
`AdminUser:Email`, `AdminUser:Password`, `ConnectionStrings:DefaultConnection` (PostgreSQL format: `Host=...;Port=5432;Database=...;Username=...;Password=...`), `CKEditor:LicenseKey`, `AutoMapper:LicenseKey`

## Migrations

Migrations live in `Comjustinspicer.CMS/Migrations/` organized by context (Identity/, Article/, ContentBlock/, ContentZone/, Page/). The startup-project is `Comjustinspicer.Web` and the migrations-project is `Comjustinspicer.CMS`:

```
dotnet ef migrations add <Name> -s Comjustinspicer.Web/Comjustinspicer.Web.csproj -p Comjustinspicer.CMS/Comjustinspicer.CMS.csproj -c <ContextName> -o Migrations/<Folder>
```
