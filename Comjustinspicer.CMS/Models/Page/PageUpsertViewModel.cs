using System.ComponentModel.DataAnnotations;
using Comjustinspicer.CMS.Attributes;

namespace Comjustinspicer.CMS.Models.Page;

public sealed class PageUpsertViewModel : BaseContentViewModel
{
    [Required]
    [FormProperty(Label = "Route", EditorType = EditorType.Text, IsRequired = true, Order = 2,
        Placeholder = "/about",
        HelpText = "Must start with \"/\", no trailing slash (except root \"/\"). Lowercase letters, numbers, hyphens, and slashes only.",
        Pattern = @"^\/[a-z0-9\-\/]*[a-z0-9\-]$|^\/$")]
    public string Route { get; set; } = string.Empty;

    [Required]
    [FormProperty(Label = "Page Controller", EditorType = EditorType.Hidden, IsRequired = true, Order = 3)]
    public string ControllerName { get; set; } = string.Empty;

    [FormProperty(EditorType = EditorType.Hidden, Order = 99)]
    public string ConfigurationJson { get; set; } = "{}";
}
