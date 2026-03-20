# CLAUDE.md

## Dev Server

- **URL:** `https://localhost:7046/`

## Commands

- **Build:** `dotnet build`
- **Run dev (hot reload + Sass watch):** `./Scripts/HotReloadRun.sh`
- **Run tests:** `./Scripts/TestsRun.sh` (loads AutoMapper license from user-secrets)
- **Run single test:** `dotnet test --filter "Name~MethodName"`
- **Rebuild Ef Migrations (destructive):** `./Scripts/RebuildEFMigrations.sh`
- **Docker build:** `./Scripts/DockerBuild.sh`

## rules
 - if you start the web project turn it off when you are done
 - when possible always use an mcp server to check all affected UI/UX
 - after finishing work check to see if documentation needs to be updated to reflect the changes
 - Do not Remove todo notes from the code unless the todo not has been completed. If you are unsure. ask
 - If Tests fail that were previously passing, do not modify those tests without permission from a human
 - When multiple good options exist ask the user which they would prefer
 - always ask clarifying questions when planning if you have any uncertainty.
 - Do not use JQuery.
 - Get confirmation from a human before using any external library or code.
 - Do not commit work
 - The very last thing you should do before existing work is reread the plan and ensure that all steps have been completed and all verification prescribed by the plan was actually done.

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
