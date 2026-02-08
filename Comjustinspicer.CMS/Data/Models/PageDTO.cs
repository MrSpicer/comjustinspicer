using Comjustinspicer.CMS.Attributes;

namespace Comjustinspicer.CMS.Data.Models;

/// <summary>
/// Database entity representing a dynamic page with a custom route and controller configuration.
/// </summary>
public class PageDTO : BaseContentDTO
{
    /// <summary>
    /// The URL route for this page (e.g., "/about", "/services/consulting").
    /// Must be unique, start with "/", and have no trailing slash.
    /// </summary>
    [FormProperty(Label = "Route", EditorType = EditorType.Text, IsRequired = true, Order = 2,
        Placeholder = "/about",
        HelpText = "Must start with \"/\", no trailing slash (except root \"/\"). Lowercase letters, numbers, hyphens, and slashes only.",
        Pattern = @"^\/[a-z0-9\-\/]*[a-z0-9\-]$|^\/$")]
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// The registered page controller name (e.g., "AboutPage").
    /// </summary>
    [FormProperty(Label = "Page Controller", EditorType = EditorType.Hidden, IsRequired = true, Order = 3)]
    public string ControllerName { get; set; } = string.Empty;

    /// <summary>
    /// JSON-serialized configuration for the page controller.
    /// </summary>
    [FormProperty(EditorType = EditorType.Hidden, Order = 99)]
    public string ConfigurationJson { get; set; } = "{}";
}
