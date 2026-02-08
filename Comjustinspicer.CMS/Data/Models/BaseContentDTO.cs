using Comjustinspicer.CMS.Attributes;

namespace Comjustinspicer.CMS.Data.Models;

public abstract class BaseContentDTO
{
    [FormProperty(EditorType = EditorType.Hidden, Order = 0)]
    public Guid Id { get; set; }

    [FormProperty(Label = "Slug", EditorType = EditorType.Text, Order = 2)]
    public string Slug { get; set; } = string.Empty;

    [FormProperty(Label = "Title", EditorType = EditorType.Text, IsRequired = true, Order = 1)]
    public string Title { get; set; } = string.Empty;

    [FormProperty(EditorType = EditorType.Hidden, Order = 99)]
    public Guid CreatedBy { get; set; }

    [FormProperty(EditorType = EditorType.Hidden, Order = 99)]
    public Guid LastModifiedBy { get; set; }

    [FormProperty(Label = "Publication Date", EditorType = EditorType.DateTime, Group = "Publishing", Order = 10)]
    public DateTime PublicationDate { get; set; }

    [FormProperty(Label = "Publication End Date", EditorType = EditorType.DateTime, Group = "Publishing", Order = 11)]
    public DateTime? PublicationEndDate { get; set; }

    [FormProperty(EditorType = EditorType.Hidden, Order = 99)]
    public DateTime ModificationDate { get; set; }

    [FormProperty(EditorType = EditorType.Hidden, Order = 99)]
    public DateTime CreationDate { get; set; }

    [FormProperty(Label = "Published", EditorType = EditorType.Checkbox, Group = "Publishing", Order = 12)]
    public bool IsPublished { get; set; }

    [FormProperty(Label = "Archived", EditorType = EditorType.Checkbox, Group = "Status", Order = 20)]
    public bool IsArchived { get; set; }

    [FormProperty(Label = "Hidden", EditorType = EditorType.Checkbox, Group = "Status", Order = 21)]
    public bool IsHidden { get; set; }

    [FormProperty(Label = "Deleted", EditorType = EditorType.Checkbox, Group = "Status", Order = 22)]
    public bool IsDeleted { get; set; }

    public List<CustomField> CustomFields { get; set; } = new();
}
