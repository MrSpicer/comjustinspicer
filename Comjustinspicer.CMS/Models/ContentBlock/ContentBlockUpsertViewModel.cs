using System.ComponentModel.DataAnnotations;

namespace Comjustinspicer.CMS.Models.ContentBlock;

public sealed class ContentBlockUpsertViewModel : BaseContentViewModel
{
    [Required]
    public string Content { get; init; } = string.Empty;
}
