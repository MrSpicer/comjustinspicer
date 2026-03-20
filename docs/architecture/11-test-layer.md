# Area 11: Test Layer

**Namespace:** `Comjustinspicer.Tests`

**Depends on:** All CMS library namespaces, NUnit 3.13.3, Moq
**Consumed by:** Nothing (leaf layer; no production code references it)

---

## 1. Test Naming Convention

All test methods follow the pattern:

```
MethodName_Scenario_ExpectedBehavior
```

Examples:
- `Upsert_Create_Read_Update_Delete_Flow`
- `TransformAsync_ExactRouteMatch_ReturnsCorrectController`
- `GetAllComponents_WithMultipleComponents_ReturnsAllRegistered`

NUnit constraint model is used exclusively:
```csharp
Assert.That(result, Is.Not.Null);
Assert.That(result.Count, Is.EqualTo(3));
Assert.That(actual, Is.EqualTo(expected));
```

---

## 2. Test Categories

| Test File | Area Covered | Type |
|-----------|-------------|------|
| `ContentBlockServiceTests.cs` | Data Tier — `ContentService<T>` CRUD flow | Integration (EF In-Memory) |
| `ContentServiceParentChildTests.cs` | Data Tier — parent/child relationships | Integration (EF In-Memory) |
| `PageServiceTests.cs` | Data Tier — `PageService`, route normalization | Integration (EF In-Memory) |
| `PostServiceTests.cs` | Data Tier — additional service scenarios | Integration (EF In-Memory) |
| `ContentZoneComponentRegistryTests.cs` | Form Generation + Content Zone Registry | Unit |
| `PageRouteTransformerTests.cs` | Page Routing Subsystem | Unit (Moq) |
| `ArticleListModelTests.cs` | Content Domain Models — `ArticleListModel` | Unit (Moq) |
| `ContentBlockModelTests.cs` | Content Domain Models — `ContentBlockModel` | Unit (Moq) |
| `PageModelTests.cs` | Content Domain Models — `PageModel` | Unit (Moq) |
| `MappingProfileTests.cs` | AutoMapper profiles — DTO ↔ ViewModel round-trip | Unit |
| `BlogModelTests.cs` | Custom content type model (site-specific) | Unit (Moq) |
| `BlogPostModelTests.cs` | Custom child model (site-specific) | Unit (Moq) |

---

## 3. Mocking Strategy

**EF Core In-Memory database** — service tests (`ContentBlockServiceTests`, `PageServiceTests`, etc.) use `UseInMemoryDatabase(Guid.NewGuid().ToString())` for each test, creating an isolated, fresh database per test. This exercises real EF Core query logic without requiring PostgreSQL.

```csharp
private DbContextOptions<ContentBlockContext> CreateNewContextOptions()
{
    return new DbContextOptionsBuilder<ContentBlockContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
}
```

**Moq** — model and routing tests mock service interfaces (`IPageService`, `IContentService<T>`, `IMapper`, etc.) to test the model/routing logic in isolation without database access.

```csharp
_pageService = new Mock<IPageService>();
_registry = new Mock<IPageControllerRegistry>();
_transformer = new PageRouteTransformer(_pageService.Object, _registry.Object);
```

**Real instances** — `ContentZoneComponentRegistryTests` and `MappingProfileTests` use real instances of the classes under test with real configuration, not mocks. The registry is tested by scanning test-specific ViewComponent classes defined inline in the test file.

---

## 4. Running Tests

**All tests:**
```bash
./Scripts/TestsRun.sh
```

The script loads the AutoMapper license key from `dotnet user-secrets` before running. Tests will fail without the license if AutoMapper mapping profile tests are included.

**Single test method:**
```bash
dotnet test --filter "Name~MethodName"
```

**AutoMapper license requirement:** `MappingProfileTests` requires the `AutoMapper:LicenseKey` secret. Store it with:
```bash
dotnet user-secrets set "AutoMapper:LicenseKey" "<key>" --project Comjustinspicer.Web
```

---

## 5. Coverage Map

| Area | Coverage |
|------|---------|
| Data Tier — `ContentService<T>` | Service tests cover full CRUD, versioning, soft delete, parent/child |
| Data Tier — `PageService` | Service tests cover route normalization, CRUD, `IsRouteAvailableAsync` |
| Data Tier — `ContentZoneService` | No dedicated tests currently |
| Form Generation — `FormPropertyBuilder` | Tested indirectly via `ContentZoneComponentRegistryTests` |
| Form Generation — `FormFieldsTagHelper` | No tests currently |
| Page Routing — `PageRouteTransformer` | Routing tests cover exact match, parent match, root fallback, sub-route, missing page |
| Page Routing — `PageControllerRegistry` | No dedicated tests; covered implicitly by `ContentZoneComponentRegistryTests` pattern |
| Content Zone Framework — `ContentZoneComponentRegistry` | Registry tests cover scanning, lookup, validation, category grouping |
| Content Domain Models — all built-in models | Model tests cover CRUD operations, version history, delete |
| Admin CRUD Framework — `AdminContentController` | No controller-level tests currently |
| CMS Bootstrap — `ServiceCollectionExtensions` | No tests (covered by integration/startup) |
| Identity | No tests |
| AutoMapper profiles | Mapping tests cover DTO → ViewModel and back for all built-in types |

Areas without tests are suitable targets for future test coverage work.
