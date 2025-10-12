using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.CMS.Models.ContentBlock;
using AutoMapper;

namespace Comjustinspicer.CMS.ViewComponents;

public class ContentBlockViewComponent : ViewComponent
{
    private readonly IContentBlockModel _model;
    private readonly IMapper _mapper;
    public ContentBlockViewComponent(IContentBlockModel model, IMapper mapper)
    {
        _model = model;
        _mapper = mapper;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid contentBlockID)
    {
        if (contentBlockID == Guid.Empty) return Content(string.Empty);
        var dto = await _model.FromIdAsync(contentBlockID, CancellationToken.None);
        if (dto == null) return View(new ContentBlockViewModel { Id = contentBlockID });
        var vm = _mapper.Map<ContentBlockViewModel>(dto);
        return View(vm);
    }
}