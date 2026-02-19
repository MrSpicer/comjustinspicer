using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.CMS.Attributes;
using Comjustinspicer.CMS.Models.ContentBlock;
using AutoMapper;

namespace Comjustinspicer.CMS.ViewComponents;

/// <summary>
/// Renders a reusable HTML content block by ID.
/// </summary>
[ContentZoneComponent(
    DisplayName = "Content Block",
    Description = "Renders a reusable HTML content block from the CMS.",
    Category = "Content",
    ConfigurationType = typeof(ContentBlockContentZoneConfiguration),
    IconClass = "fa-file-alt",
    Order = 1
)]
public class ContentBlockViewComponent : ViewComponent
{
    private readonly IContentBlockModel _model;
    private readonly IMapper _mapper;
    public ContentBlockViewComponent(IContentBlockModel model, IMapper mapper)
    {
        _model = model;
        _mapper = mapper;
    }

    public async Task<IViewComponentResult> InvokeAsync(ContentBlockContentZoneConfiguration config)
    {
        if (config == null || config.ContentBlockID == Guid.Empty) 
            return Content(string.Empty);
        
        var dto = await _model.FromMasterIdAsync(config.ContentBlockID, CancellationToken.None);
        if (dto == null) 
            return View(new ContentBlockViewModel { Id = config.ContentBlockID });
        
        var vm = _mapper.Map<ContentBlockViewModel>(dto);
        return View(vm);
    }
}