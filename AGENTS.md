# AGENTS.md

Guidelines for AI coding assistants working in this repository.

## Project Overview

- **Framework:** ASP.NET Core MVC on .NET 10.0 (C#)
- **Architecture:** Multi-project solution with 3 projects
- **Database:** SQLite with Entity Framework Core 10.0
- **Logging:** Serilog
- **Testing:** NUnit 3.13.3 with Moq
- **Mapping:** AutoMapper 15.0.1

## Project Structure

```
comjustinspicer.sln
├── Comjustinspicer.Web/       # Main web application (entry point)
│   ├── Controllers/           # MVC controllers
│   ├── Views/                 # Razor views
│   └── wwwroot/               # Static assets
├── Comjustinspicer.CMS/       # Reusable CMS class library
│   ├── Areas/Identity/        # ASP.NET Identity customization
│   ├── ContentZones/          # Content zone component system
│   ├── Controllers/Admin/     # Admin controllers
│   ├── Data/Models/           # EF Core entity models
│   ├── Data/Services/         # Data services
│   ├── Migrations/            # EF Core migrations
│   ├── Models/                # ViewModels and DTOs
│   └── ViewComponents/        # Razor view components
└── Comjustinspicer.Tests/     # Unit tests (NUnit)
```

## Build Commands

```bash
# Restore packages
dotnet restore

# Build (Debug)
dotnet build

# Build (Release)
dotnet build -c Release

# Run with hot reload (development)
./Scripts/HotReloadRun.sh
# Or directly:
dotnet watch run --project Comjustinspicer.Web/Comjustinspicer.Web.csproj
```

## Test Commands

```bash
# Run all tests
dotnet test

# Run all tests with detailed output
dotnet test --verbosity normal

# Run tests in specific project
dotnet test --project Comjustinspicer.Tests
```

### Running a Single Test

```bash
# By test method name (partial match)
dotnet test --filter "Name~MethodName"

# By class name (partial match)
dotnet test --filter "FullyQualifiedName~ClassName"

# By exact fully qualified name
dotnet test --filter "FullyQualifiedName=Comjustinspicer.Tests.ClassName.MethodName"

# By category (if using [Category] attribute)
dotnet test --filter "TestCategory=CategoryName"
```

## Database Migrations

```bash
# Apply all migrations
./Scripts/ApplyMigrations.sh

# Apply migrations for specific context
dotnet ef database update -c ArticleContext --project Comjustinspicer.CMS
dotnet ef database update -c ApplicationDbContext --project Comjustinspicer.CMS
dotnet ef database update -c ContentBlockContext --project Comjustinspicer.CMS
```

## Code Style Guidelines

### Namespaces

Use C# 10+ file-scoped namespaces:
```csharp
namespace Comjustinspicer.CMS.Models.Article;  // Correct
// NOT: namespace Comjustinspicer.CMS.Models.Article { }
```

### Nullable Reference Types

Nullable reference types are enabled project-wide. Use nullable annotations:
```csharp
public string? OptionalField { get; set; }
public string RequiredField { get; set; } = string.Empty;
```

### Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Files (classes) | PascalCase, matches class | `ArticleModel.cs` |
| Files (interfaces) | `I` prefix | `IContentService.cs` |
| Files (DTOs) | `{Name}DTO.cs` | `PostDTO.cs` |
| Files (ViewModels) | `{Name}ViewModel.cs` | `ArticleViewModel.cs` |
| Files (Controllers) | `{Name}Controller.cs` | `BlogController.cs` |
| Files (Tests) | `{Name}Tests.cs` | `ContentZoneTests.cs` |
| Private fields | `_camelCase` | `_logger`, `_dbContext` |
| Local variables | `camelCase` | `connectionString` |
| Parameters | `camelCase` | `componentName` |
| Public members | `PascalCase` | `GetAllAsync()` |
| Async methods | Suffix with `Async` | `CreateAsync()` |
| Interfaces | `I` prefix | `IContentService<T>` |

### Import Organization

Order imports as follows:
1. System namespaces
2. Microsoft/Framework namespaces
3. Third-party libraries (AutoMapper, Serilog)
4. Project namespaces

### Error Handling

Use guard clauses with null checks:
```csharp
public MyClass(IService service)
{
    _service = service ?? throw new ArgumentNullException(nameof(service));
}
```

Use null coalescing for configuration:
```csharp
var connString = config.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string not found.");
```

Return tuples for operations that can fail:
```csharp
Task<(bool Success, string? ErrorMessage)> SaveAsync(...)
```

### Async Patterns

- Always suffix async methods with `Async`
- Include `CancellationToken` parameter with default value:
```csharp
public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
```

### Dependency Injection

Use constructor injection with null guards:
```csharp
public sealed class MyService
{
    private readonly ILogger _logger;
    private readonly IDbContext _dbContext;

    public MyService(ILogger logger, IDbContext dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
}
```

### Documentation

Use XML documentation on public APIs:
```csharp
/// <summary>
/// Creates a new article with the specified content.
/// </summary>
/// <param name="dto">The article data transfer object.</param>
/// <returns>The created article ID, or null if creation failed.</returns>
public async Task<Guid?> CreateAsync(ArticleDTO dto)
```

### Testing (NUnit)

Test class structure:
```csharp
[TestFixture]
public class MyServiceTests
{
    private IMyService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new MyService(...);
    }

    [Test]
    public void MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange, Act, Assert
    }
}
```

Use NUnit constraint model for assertions:
```csharp
Assert.That(result, Is.Not.Null);
Assert.That(result.Count, Is.EqualTo(3));
Assert.That(names, Does.Contain("Expected"));
```

### Controllers

Use attribute routing and authorize attributes:
```csharp
[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController : Controller
{
    private readonly ILogger _logger = Log.ForContext<AdminController>();

    [HttpGet("articles")]
    public async Task<IActionResult> Index() { }

    [HttpPost("articles/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id) { }
}
```

## CI/CD

GitHub Actions runs on pushes/PRs to `master`:
1. `dotnet restore`
2. `dotnet build --no-restore -c Release`
3. `dotnet test --no-build -c Release --verbosity normal`
4. `docker build`

## Development Setup

Required secrets (use `dotnet user-secrets`):
- `AdminUser:Email`
- `AdminUser:Password`
- `ConnectionStrings:DefaultConnection`
- `CKEditor:LicenseKey`
- `AutoMapper:LicenseKey`

See README.md for full setup instructions.
