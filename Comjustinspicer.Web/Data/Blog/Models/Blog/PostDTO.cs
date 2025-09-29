namespace comjustinspicer.Data.Models.Blog;

public class PostDTO
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Body { get; set; } = string.Empty;
	public DateTime PublicationDate { get; set; }
	public string AuthorName {get; set;} = string.Empty;
	public DateTime ModificationDate {get; set;}
	public DateTime CreationDate {get; set;}
}