using Comjustinspicer.CMS.Attributes;

namespace Comjustinspicer.CMS.Models.Page;

public class PageContentZoneConfiguration
{
    [FormProperty(
        Label = "View Name",
        HelpText = "The view template to use. Leave empty for default behavior.",
        Placeholder = "e.g., Default",
        EditorType = EditorType.ViewPicker,
        ViewComponentName = "Page",
        Order = 1
    )]
    public string? ViewName { get; set; }
}
