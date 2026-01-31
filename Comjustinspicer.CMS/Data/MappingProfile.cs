using AutoMapper;
using Comjustinspicer.CMS.Data.Models;
using Comjustinspicer.CMS.Models.Article;
using Comjustinspicer.CMS.Models.ContentBlock;

namespace Comjustinspicer.CMS.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
    // ContentBlock mappings
    CreateMap<ContentBlockDTO, ContentBlockViewModel>()
            .ForMember(d => d.Content, opt => opt.MapFrom(s => s.Content ?? string.Empty));

    // Post mappings
    CreateMap<PostDTO, ArticleViewModel>();
    CreateMap<PostDTO, PostUpsertViewModel>()
        .ForMember(d => d.PublicationDate, opt => opt.MapFrom(s => s.PublicationDate == default ? (DateTime?)null : s.PublicationDate));

    CreateMap<PostUpsertViewModel, PostDTO>()
        .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.HasValue && s.Id != Guid.Empty ? s.Id!.Value : Guid.NewGuid()))
        .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title ?? string.Empty))
        .ForMember(d => d.Body, opt => opt.MapFrom(s => s.Body ?? string.Empty))
        .ForMember(d => d.AuthorName, opt => opt.MapFrom(s => s.AuthorName ?? string.Empty))
        .ForMember(d => d.PublicationDate, opt => opt.MapFrom(s => s.PublicationDate ?? DateTime.UtcNow))
        .ForMember(d => d.CreationDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
        .ForMember(d => d.ModificationDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
        .ForMember(d => d.IsPublished, opt => opt.MapFrom(s => s.IsPublished))
        //todo: add these to the view model
        .ForMember(d => d.CreatedBy, opt => opt.Ignore())
        .ForMember(d => d.LastModifiedBy, opt => opt.Ignore())
        .ForMember(d => d.IsArchived, opt => opt.Ignore())
        .ForMember(d => d.IsHidden, opt => opt.Ignore())
        .ForMember(d => d.IsDeleted, opt => opt.Ignore())
        .ForMember(d => d.PublicationEndDate, opt => opt.Ignore());
  }
}