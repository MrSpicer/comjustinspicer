namespace Comjustinspicer.CMS.Models.Article;


public sealed class ArticleViewModel
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTime PublicationDate { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public DateTime ModificationDate { get; init; }
    public DateTime CreationDate { get; init; }
    public bool IsPublished { get; init; }

    //todo: delete?
    // Parameterless for AutoMapper
    public ArticleViewModel() { }
}