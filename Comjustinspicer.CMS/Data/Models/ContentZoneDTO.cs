using Comjustinspicer.CMS.Attributes;

namespace Comjustinspicer.CMS.Data.Models;

/// <summary>
/// Database entity representing a named content zone that can contain multiple zone items.
/// </summary>
public class ContentZoneDTO : BaseContentDTO
{
    /// <summary>
    /// Unique name/identifier for this content zone, used to reference it in views.
    /// </summary>
    [FormProperty(Label = "Name", EditorType = EditorType.Text, IsRequired = true, Order = 1,
        Placeholder = "e.g., homepage-hero, sidebar-widgets",
        HelpText = "This is the name used to reference this zone in your views.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of what this content zone is used for.
    /// </summary>
    [FormProperty(Label = "Description", EditorType = EditorType.TextArea, Order = 3,
        Placeholder = "Optional description of this zone's purpose")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property for zone items.
    /// </summary>
    public List<ContentZoneItemDTO> Items { get; set; } = new();
}
