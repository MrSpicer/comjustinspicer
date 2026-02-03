namespace Comjustinspicer.CMS.Data.Models;

public class CustomField
{
    public Guid Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
