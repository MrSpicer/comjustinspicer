using Comjustinspicer.CMS.Attributes;

namespace Comjustinspicer.CMS.Data.Models;

/// <summary>
/// Database entity representing an item within a content zone.
/// Each item references a view component to render with specific properties.
/// </summary>
public class ContentZoneItemDTO
{
    [FormProperty(EditorType = EditorType.Hidden, Order = 0)]
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the parent ContentZone.
    /// </summary>
    [FormProperty(EditorType = EditorType.Hidden, Order = 0)]
    public Guid ContentZoneId { get; set; }

    /// <summary>
    /// Navigation property to the parent zone.
    /// </summary>
    public ContentZoneDTO? ContentZone { get; set; }

    /// <summary>
    /// Display order within the content zone (lower numbers render first).
    /// </summary>
    [FormProperty(Label = "Ordinal", EditorType = EditorType.Number, Order = 3, Min = 0,
        HelpText = "Lower numbers appear first. Leave at 0 to auto-assign.")]
    public int Ordinal { get; set; }

    /// <summary>
    /// Name of the ViewComponent to render (e.g., "ContentBlock", "Article").
    /// </summary>
    [FormProperty(Label = "Component Name", EditorType = EditorType.Text, IsRequired = true, Order = 1,
        Placeholder = "e.g., ContentBlock, Article, Layout",
        HelpText = "The name of the ViewComponent to render (without \"ViewComponent\" suffix).")]
    public string ComponentName { get; set; } = string.Empty;

    /// <summary>
    /// JSON-serialized properties to pass to the ViewComponent.
    /// </summary>
    [FormProperty(Label = "Component Properties (JSON)", EditorType = EditorType.TextArea, IsRequired = true, Order = 2,
        Placeholder = "{\"contentBlockID\": \"00000000-0000-0000-0000-000000000000\"}",
        HelpText = "JSON object with properties to pass to the ViewComponent.")]
    public string ComponentPropertiesJson { get; set; } = "{}";

    /// <summary>
    /// Whether this item is active and should be rendered.
    /// </summary>
    [FormProperty(Label = "Active", EditorType = EditorType.Checkbox, Order = 4,
        HelpText = "Only active items will be rendered on the site.")]
    public bool IsActive { get; set; } = true;

    [FormProperty(EditorType = EditorType.Hidden, Order = 99)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [FormProperty(EditorType = EditorType.Hidden, Order = 99)]
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}
