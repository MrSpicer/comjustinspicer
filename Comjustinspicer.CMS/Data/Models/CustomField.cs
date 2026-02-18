namespace Comjustinspicer.CMS.Data.Models;

public record CustomField : BaseContentDTO
{
    public string FieldName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
