# Widget System (Content Zones)

Content Zones are named, database-backed regions in a view that an admin can populate with **widgets** at runtime through the CMS admin UI — no code deploys required.

## Table of Contents

- [System Overview](#system-overview)
- [Core Components](#core-components)
- [Placing a Zone in a View](#placing-a-zone-in-a-view)
- [How to Add a New Widget](#how-to-add-a-new-widget)

---

## System Overview

- A **Content Zone** is a named slot in a Razor view. Each zone stores an ordered list of widget instances in the database.
- The `ContentZone` view component renders all widgets assigned to a zone path. When an admin is viewing the page it also renders an inline "Add Widget" button and edit controls.
- **Widgets** are ViewComponent classes decorated with `[ContentZoneComponent]`. They are discovered automatically at startup by `ContentZoneComponentRegistry`, which scans both the CMS assembly and the entry assembly (`Comjustinspicer.Web`).
- Each widget can declare a typed **configuration class**. Properties on that class decorated with `[FormProperty]` are rendered as form fields in the admin "Add Widget" modal.

---

## Core Components

| Class | File | Role |
|---|---|---|
| `ContentZoneViewComponent` | `Comjustinspicer.CMS/ViewComponents/ContentZoneViewComponent.cs` | Renders a zone by name; switches to edit view for admins |
| `ContentZoneComponentRegistry` | `Comjustinspicer.CMS/ContentZones/ContentZoneComponentRegistry.cs` | Scans assemblies and caches widget metadata at startup |
| `[ContentZoneComponent]` | `Comjustinspicer.CMS/Attributes/ContentZoneComponentAttribute.cs` | Marks a ViewComponent as a widget available in the admin UI |
| `[FormProperty]` / `EditorType` | `Comjustinspicer.CMS/Attributes/FormPropertyAttribute.cs` | Drives config form field generation in the admin UI |

---

## Placing a Zone in a View

```cshtml
@* Page-scoped zone — unique per page *@
@await Component.InvokeAsync("ContentZone", new { zoneName = "Hero" })

@* Global zone — shared across all pages (nav, footer, etc.) *@
@await Component.InvokeAsync("ContentZone", new { zoneName = "Sidebar", IsGlobal = true })
```

- `zoneName` — logical name for the zone; combined with the current page/route context to form a unique path stored in the database.
- `IsGlobal = true` — bypasses the page context so one zone instance is shared across all pages.

In normal (non-admin) mode the component renders nothing if the zone has no items assigned.

---

## How to Add a New Widget

All files live in **`Comjustinspicer.Web`**. No changes to the CMS library are needed.

### Step 1 — (Optional) Create a configuration class

**`Comjustinspicer.Web/ViewComponents/MyWidgetConfiguration.cs`**

Properties decorated with `[FormProperty]` appear as form fields in the admin "Add Widget" modal. Omit this class entirely if the widget has no configuration.

```csharp
using Comjustinspicer.CMS.Attributes;

namespace Comjustinspicer.Web.ViewComponents;

public class MyWidgetConfiguration
{
    [FormProperty(Label = "Heading", EditorType = EditorType.Text, Order = 1)]
    public string Heading { get; set; } = string.Empty;

    [FormProperty(Label = "Show Border", EditorType = EditorType.Checkbox, Order = 2)]
    public bool ShowBorder { get; set; }
}
```

Available `EditorType` values:

| Value | Editor rendered |
|---|---|
| `Text` | Single-line text input |
| `TextArea` | Multi-line textarea |
| `RichText` | Rich text / HTML editor |
| `Number` | Numeric input |
| `Checkbox` | Boolean checkbox |
| `Guid` | GUID input with optional entity picker |
| `Dropdown` | Select from predefined options |
| `Date` | Date picker |
| `DateTime` | Date + time picker |
| `Color` | Color picker |
| `Url` | URL input with validation |
| `Email` | Email input with validation |
| `ViewPicker` | Dropdown of available views for the component |
| `Hidden` | Hidden field (included in config, not shown) |

### Step 2 — Create the ViewComponent

**`Comjustinspicer.Web/ViewComponents/MyWidgetViewComponent.cs`**

```csharp
using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.CMS.Attributes;

namespace Comjustinspicer.Web.ViewComponents;

[ContentZoneComponent(
    DisplayName = "My Widget",
    Description = "Displays a custom widget.",
    Category = "General",
    ConfigurationType = typeof(MyWidgetConfiguration),
    IconClass = "fa-star",
    Order = 10
)]
public class MyWidgetViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(MyWidgetConfiguration config)
    {
        config ??= new MyWidgetConfiguration();
        return View(config);
    }
}
```

`[ContentZoneComponent]` properties:

| Property | Description |
|---|---|
| `DisplayName` | Label shown in the admin "Add Widget" dropdown |
| `Description` | Help text shown in the admin UI |
| `Category` | Groups widgets in the dropdown (e.g. `"General"`, `"Content"`, `"Navigation"`) |
| `ConfigurationType` | The config class from Step 1; omit if no config is needed |
| `IconClass` | Font Awesome class for the admin UI icon (e.g. `"fa-star"`) |
| `Order` | Sort order within the category; lower values appear first |

### Step 3 — Create the Razor view

**`Comjustinspicer.Web/Views/Shared/Components/MyWidget/Default.cshtml`**

```cshtml
@model Comjustinspicer.Web.ViewComponents.MyWidgetConfiguration

<div class="my-widget @(Model.ShowBorder ? "bordered" : "")">
    <h2>@Model.Heading</h2>
</div>
```

Additional named views (e.g. `Compact.cshtml`) can be added in the same folder and selected via a `ViewPicker` config property.

### Step 4 — No registration required

`ContentZoneComponentRegistry` scans `Assembly.GetEntryAssembly()` (i.e. `Comjustinspicer.Web`) automatically at startup. No changes to `ServiceCollectionExtensions.cs` or `Program.cs` are needed.

---

## How Zone Paths Work

The `ContentZoneViewComponent` builds a unique database path for each zone instance at render time:

- **Page-scoped zones** use the page's `MasterId`: `page:{masterId}/{zoneName}#{ordinal}`
- **Global zones** use a `"Global"` prefix: `Global/{zoneName}#{ordinal}`
- The `#ordinal` suffix ensures that multiple invocations of the same zone name on a single page each get their own independent slot.

This path is what is stored as the `Name` on the `ContentZoneDTO` record and is transparent to widget authors.
