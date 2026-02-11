using AutoMapper;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Models.Article;
using Comjustinspicer.CMS.Models.ContentBlock;
using Comjustinspicer.CMS.Models.Page;

namespace Comjustinspicer.CMS.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
    // ContentBlock mappings
    CreateMap<ContentBlockDTO, ContentBlockViewModel>()
            .ForMember(d => d.Slug, opt => opt.MapFrom(s => s.Slug ?? string.Empty))
            .ForMember(d => d.Content, opt => opt.MapFrom(s => s.Content ?? string.Empty));

    CreateMap<ContentBlockDTO, ContentBlockUpsertViewModel>()
        .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title ?? string.Empty))
        .ForMember(d => d.Slug, opt => opt.MapFrom(s => s.Slug ?? string.Empty))
        .ForMember(d => d.Content, opt => opt.MapFrom(s => s.Content ?? string.Empty));

    CreateMap<ContentBlockUpsertViewModel, ContentBlockDTO>()
        .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.HasValue && s.Id != Guid.Empty ? s.Id!.Value : Guid.NewGuid()))
        .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title ?? string.Empty))
        .ForMember(d => d.Slug, opt => opt.MapFrom(s => Uri.EscapeDataString(s.Slug ?? string.Empty)))
        .ForMember(d => d.Content, opt => opt.MapFrom(s => s.Content ?? string.Empty))
        .ForMember(d => d.CreationDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
        .ForMember(d => d.ModificationDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
        .ForMember(d => d.PublicationDate, opt => opt.MapFrom(s => s.PublicationDate ?? DateTime.UtcNow))
        .ForMember(d => d.PublicationEndDate, opt => opt.MapFrom(s => s.PublicationEndDate))
        .ForMember(d => d.IsPublished, opt => opt.MapFrom(s => s.IsPublished))
        .ForMember(d => d.IsArchived, opt => opt.MapFrom(s => s.IsArchived))
        .ForMember(d => d.IsHidden, opt => opt.MapFrom(s => s.IsHidden))
        .ForMember(d => d.IsDeleted, opt => opt.MapFrom(s => s.IsDeleted));

    // Post mappings
    CreateMap<PostDTO, ArticleViewModel>()
        .ForMember(d => d.ArticleListId, opt => opt.MapFrom(s => s.ArticleListId));
    CreateMap<PostDTO, ArticleUpsertViewModel>()
        .ForMember(d => d.Slug, opt => opt.MapFrom(s => s.Slug ?? string.Empty))
        .ForMember(d => d.ArticleListId, opt => opt.MapFrom(s => s.ArticleListId))
        .ForMember(d => d.PublicationDate, opt => opt.MapFrom(s => s.PublicationDate == default ? (DateTime?)null : s.PublicationDate));

    CreateMap<ArticleUpsertViewModel, PostDTO>()
        .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.HasValue && s.Id != Guid.Empty ? s.Id!.Value : Guid.NewGuid()))
        .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title ?? string.Empty))
        .ForMember(d => d.Slug, opt => opt.MapFrom(s => Uri.EscapeDataString(s.Slug ?? string.Empty)))
        .ForMember(d => d.Body, opt => opt.MapFrom(s => s.Body ?? string.Empty))
        .ForMember(d => d.AuthorName, opt => opt.MapFrom(s => s.AuthorName ?? string.Empty))
        .ForMember(d => d.Summary, opt => opt.MapFrom(s => s.Summary ?? string.Empty))
        .ForMember(d => d.ArticleListId, opt => opt.MapFrom(s => s.ArticleListId))
        .ForMember(d => d.ArticleList, opt => opt.Ignore())
        .ForMember(d => d.PublicationDate, opt => opt.MapFrom(s => s.PublicationDate ?? DateTime.UtcNow))
        .ForMember(d => d.PublicationEndDate, opt => opt.MapFrom(s => s.PublicationEndDate))
        .ForMember(d => d.CreationDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
        .ForMember(d => d.ModificationDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
        .ForMember(d => d.IsPublished, opt => opt.MapFrom(s => s.IsPublished))
        .ForMember(d => d.IsArchived, opt => opt.MapFrom(s => s.IsArchived))
        .ForMember(d => d.IsHidden, opt => opt.MapFrom(s => s.IsHidden))
        .ForMember(d => d.IsDeleted, opt => opt.MapFrom(s => s.IsDeleted));

    // ArticleList mappings
    CreateMap<ArticleListDTO, ArticleListUpsertViewModel>()
        .ForMember(d => d.Slug, opt => opt.MapFrom(s => s.Slug ?? string.Empty))
        .ForMember(d => d.PublicationDate, opt => opt.MapFrom(s => s.PublicationDate == default ? (DateTime?)null : s.PublicationDate));

    CreateMap<ArticleListUpsertViewModel, ArticleListDTO>()
        .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.HasValue && s.Id != Guid.Empty ? s.Id!.Value : Guid.NewGuid()))
        .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title ?? string.Empty))
        .ForMember(d => d.Slug, opt => opt.MapFrom(s => Uri.EscapeDataString(s.Slug ?? string.Empty)))
        .ForMember(d => d.Articles, opt => opt.Ignore())
        .ForMember(d => d.CreationDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
        .ForMember(d => d.ModificationDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
        .ForMember(d => d.PublicationDate, opt => opt.MapFrom(s => s.PublicationDate ?? DateTime.UtcNow))
        .ForMember(d => d.PublicationEndDate, opt => opt.MapFrom(s => s.PublicationEndDate))
        .ForMember(d => d.IsPublished, opt => opt.MapFrom(s => s.IsPublished))
        .ForMember(d => d.IsArchived, opt => opt.MapFrom(s => s.IsArchived))
        .ForMember(d => d.IsHidden, opt => opt.MapFrom(s => s.IsHidden))
        .ForMember(d => d.IsDeleted, opt => opt.MapFrom(s => s.IsDeleted));

    CreateMap<ArticleListDTO, ArticleListItemViewModel>()
        .ForMember(d => d.Slug, opt => opt.MapFrom(s => s.Slug ?? string.Empty))
        .ForMember(d => d.ArticleCount, opt => opt.Ignore());

    // Page mappings
    CreateMap<PageDTO, PageUpsertViewModel>()
        .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title ?? string.Empty))
        .ForMember(d => d.Slug, opt => opt.MapFrom(s => s.Slug ?? string.Empty))
        .ForMember(d => d.Route, opt => opt.MapFrom(s => s.Route ?? string.Empty))
        .ForMember(d => d.ControllerName, opt => opt.MapFrom(s => s.ControllerName ?? string.Empty))
        .ForMember(d => d.ConfigurationJson, opt => opt.MapFrom(s => s.ConfigurationJson ?? "{}"))
        .ForMember(d => d.PublicationDate, opt => opt.MapFrom(s => s.PublicationDate == default ? (DateTime?)null : s.PublicationDate));

    CreateMap<PageUpsertViewModel, PageDTO>()
        .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.HasValue && s.Id != Guid.Empty ? s.Id!.Value : Guid.NewGuid()))
        .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title ?? string.Empty))
        .ForMember(d => d.Slug, opt => opt.MapFrom(s => Uri.EscapeDataString(s.Slug ?? string.Empty)))
        .ForMember(d => d.Route, opt => opt.MapFrom(s => s.Route ?? string.Empty))
        .ForMember(d => d.ControllerName, opt => opt.MapFrom(s => s.ControllerName ?? string.Empty))
        .ForMember(d => d.ConfigurationJson, opt => opt.MapFrom(s => s.ConfigurationJson ?? "{}"))
        .ForMember(d => d.CreationDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
        .ForMember(d => d.ModificationDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
        .ForMember(d => d.PublicationDate, opt => opt.MapFrom(s => s.PublicationDate ?? DateTime.UtcNow))
        .ForMember(d => d.PublicationEndDate, opt => opt.MapFrom(s => s.PublicationEndDate))
        .ForMember(d => d.IsPublished, opt => opt.MapFrom(s => s.IsPublished))
        .ForMember(d => d.IsArchived, opt => opt.MapFrom(s => s.IsArchived))
        .ForMember(d => d.IsHidden, opt => opt.MapFrom(s => s.IsHidden))
        .ForMember(d => d.IsDeleted, opt => opt.MapFrom(s => s.IsDeleted));

    CreateMap<PageDTO, PageItemViewModel>();

    // ContentBlock index item mapping
    CreateMap<ContentBlockDTO, ContentBlockItemViewModel>();
  }
}
