using System.ComponentModel.DataAnnotations;
using Comjustinspicer.CMS.Attributes;

namespace Comjustinspicer.CMS.Models.ContentZone;

public sealed class ContentZoneUpsertViewModel : BaseContentViewModel
{
    [Required]
    [FormProperty(Label = "Name", EditorType = EditorType.Text, IsRequired = true, Order = 3)]
    public string Name { get; init; } = string.Empty;

    [FormProperty(Label = "Description", EditorType = EditorType.TextArea, Order = 4)]
    public string Description { get; init; } = string.Empty;
}
