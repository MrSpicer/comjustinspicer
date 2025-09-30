using System.ComponentModel.DataAnnotations;
using comjustinspicer.Data.Blog.Models;

namespace comjustinspicer.Models.Blog;

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

    public static PostUpsertViewModel FromDto(PostDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new PostUpsertViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Body = dto.Body,
            PublicationDate = dto.PublicationDate == default ? null : dto.PublicationDate,
            AuthorName = dto.AuthorName
        };
    }

    public PostDTO ToDto()
    {
        var id = Id ?? Guid.NewGuid();

        return new PostDTO
        {
            Id = id,
            Title = Title ?? string.Empty,
            Body = Body ?? string.Empty,
            PublicationDate = PublicationDate ?? DateTime.UtcNow,
            AuthorName = AuthorName ?? string.Empty,
            CreationDate = DateTime.UtcNow,
            ModificationDate = DateTime.UtcNow
        };
    }
}
