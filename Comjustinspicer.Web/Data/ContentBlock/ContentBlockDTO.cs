namespace comjustinspicer.Data.Models.ContentBlock;

public class ContentBlockDTO
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public DateTime ModificationDate { get; set; }
	public DateTime CreationDate { get; set; }
}