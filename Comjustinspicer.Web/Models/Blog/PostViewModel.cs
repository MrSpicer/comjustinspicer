using System;
using Comjustinspicer.Data.Blog.Models;

namespace Comjustinspicer.Models.Blog;


public sealed class PostViewModel
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTime PublicationDate { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public DateTime ModificationDate { get; init; }
    public DateTime CreationDate { get; init; }
    // Parameterless for AutoMapper
    public PostViewModel() { }
}