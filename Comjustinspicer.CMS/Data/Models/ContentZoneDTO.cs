namespace Comjustinspicer.CMS.Data.Models;

/// <summary>
/// Database entity representing a named content zone that can contain multiple zone items.
/// </summary>
public class ContentZoneDTO : BaseContentDTO
{
    /// <summary>
    /// Unique name/identifier for this content zone, used to reference it in views.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of what this content zone is used for.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property for zone items.
    /// </summary>
    public List<ContentZoneItemDTO> Items { get; set; } = new();
}
