using System.ComponentModel.DataAnnotations;

namespace Comjustinspicer.Models.Blog;

public sealed class PostUpsertViewModel
{
    public Guid? Id { get; init; }

    [Required]
    [StringLength(500, ErrorMessage = "Title cannot be longer than 500 characters.")]
    public string Title { get; init; } = string.Empty;

    [Required]
    public string Body { get; init; } = string.Empty;

    [Display(Name = "Publication Date")]
    public DateTime? PublicationDate { get; init; }

    [Display(Name = "Author")]
    [StringLength(200)]
    public string AuthorName { get; init; } = string.Empty;

}
