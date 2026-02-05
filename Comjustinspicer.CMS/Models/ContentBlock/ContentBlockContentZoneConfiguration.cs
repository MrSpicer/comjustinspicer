using System.ComponentModel.DataAnnotations;
using Comjustinspicer.CMS.Attributes;

namespace Comjustinspicer.CMS.Models.ContentBlock;

/// <summary>
/// Configuration model for the ContentBlock ViewComponent when used within a content zone.
/// Defines the properties that can be configured in the admin UI.
/// </summary>
public class ContentBlockContentZoneConfiguration
{
    /// <summary>
    /// Gets or sets the ID of the content block to render.
    /// </summary>
    [ContentZoneProperty(
        Label = "Content Block",
        HelpText = "Select the content block to display in this zone.",
        EditorType = EditorType.Guid,
        EntityType = "ContentBlock",
        IsRequired = true,
        Order = 1
    )]
    [Required(ErrorMessage = "Please select a content block.")]
    public Guid ContentBlockID { get; set; }

    [ContentZoneProperty(
    Label = "View Name",
    HelpText = "The view template to use. Leave empty for default behavior.",
    Placeholder = "e.g., Post, Default, Summary",
    EditorType = EditorType.ViewPicker,
    ViewComponentName = "ContentBlock",
    Order = 2
    )]
    public string? ViewName { get; set; }
}
