using System;
using comjustinspicer.Data.Models.Blog;

namespace comjustinspicer.Models.Blog;

public sealed class PostViewModel
{
	public Guid Id { get; init; }
	public string Title { get; init; } = string.Empty;
	public string Body { get; init; } = string.Empty;
	public DateTime PublicationDate { get; init; }
	public string AuthorName { get; init; } = string.Empty;
	public DateTime ModificationDate { get; init; }
	public DateTime CreationDate { get; init; }

	public PostViewModel(PostDTO post)
	{
		if (post == null) throw new ArgumentNullException(nameof(post));

		Id = post.Id;
		Title = post.Title;
		Body = post.Body;
		PublicationDate = post.PublicationDate;
		AuthorName = post.AuthorName;
		ModificationDate = post.ModificationDate;
		CreationDate = post.CreationDate;
	}
}