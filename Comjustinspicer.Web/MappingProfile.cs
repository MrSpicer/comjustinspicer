using AutoMapper;
using System; // for DateTime
using Comjustinspicer.CMS.Data.Blog.Models;
using Comjustinspicer.CMS.Data.ContentBlock.Models;
using Comjustinspicer.Models.Blog;
using Comjustinspicer.CMS.Models.ContentBlock;

namespace Comjustinspicer;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Post mappings
        CreateMap<PostDTO, PostViewModel>();
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
			//todo: fix this is published. maybe map created by.
            // Unmapped base properties
			.ForMember(d => d.IsPublished, opt => opt.MapFrom(s => (s.PublicationDate ?? DateTime.UtcNow) <= DateTime.UtcNow))
            // CreatedBy / LastModifiedBy come from authenticated user context; ignore here so domain/service layer can populate
            .ForMember(d => d.CreatedBy, opt => opt.Ignore())
            .ForMember(d => d.LastModifiedBy, opt => opt.Ignore());
    }
}