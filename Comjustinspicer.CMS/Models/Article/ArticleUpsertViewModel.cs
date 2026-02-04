using System.ComponentModel.DataAnnotations;

namespace Comjustinspicer.CMS.Models.Article;

public sealed class ArticleUpsertViewModel : BaseContentViewModel
{
    [Required]
    public string Body { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    [Display(Name = "Author")]
    [StringLength(200)]
    public string AuthorName { get; init; } = string.Empty;
}
