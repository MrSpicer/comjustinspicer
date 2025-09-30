namespace comjustinspicer.Data.Models;

public abstract class BaseContentDTO
{
	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public DateTime PublicationDate { get; set; }
	public DateTime ModificationDate { get; set; }
	public DateTime CreationDate { get; set; }
}