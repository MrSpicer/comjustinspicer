using AutoMapper;
using System; // for DateTime
using Comjustinspicer.CMS.Data.Models;
// using Comjustinspicer.CMS.Models.Blog;
using Comjustinspicer.CMS.Models.ContentBlock;

namespace Comjustinspicer.CMS.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
		// ContentBlock mappings
		CreateMap<ContentBlockDTO, ContentBlockViewModel>()
            .ForMember(d => d.Content, opt => opt.MapFrom(s => s.Content ?? string.Empty));
    }
}