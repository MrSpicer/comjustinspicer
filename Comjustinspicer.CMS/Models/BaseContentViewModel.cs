using System.ComponentModel.DataAnnotations;

namespace Comjustinspicer.CMS.Models;

/// <summary>
/// Base view model for content types that map from BaseContentDTO.
/// Contains common properties shared across all content view models.
/// </summary>
public abstract class BaseContentViewModel
{
    public Guid? Id { get; init; }
    [Required]
    [StringLength(500, ErrorMessage = "Title cannot be longer than 500 characters.")]
    public string Title { get; init; } = string.Empty;
    [StringLength(500, ErrorMessage = "Slug cannot be longer than 500 characters.")]
    public string? Slug { get; init; }
    public DateTime? PublicationDate { get; set; }
    public DateTime? PublicationEndDate { get; set; }
    public DateTime? ModificationDate { get; init; }
    public DateTime? CreationDate { get; init; }
    public bool IsPublished { get; set; }
    public bool IsArchived { get; set; }
    public bool IsHidden { get; set; }
    public bool IsDeleted { get; set; }

    //todo: custom fields. List<object> maybe with the field value cast to the type
}
