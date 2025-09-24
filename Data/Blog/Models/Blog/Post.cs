namespace comjustinspicer.Data.Models.Blog;

public class Post
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
		//public DateTime PublicationDate {get; set;}
		//public string AuthorName {get; set;}
		//public DateTime ModificationDate {get; set;}
		//public DateTime CreationDate {get; set;}
		
	public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}