using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.CMS.Models.Article;
using AutoMapper;

namespace Comjustinspicer.CMS.ViewComponents;

public class ArticleViewComponent : ViewComponent
{
    private readonly IArticleListModel _model;
    private readonly IMapper _mapper;
    public ArticleViewComponent(IArticleListModel model, IMapper mapper)
    {
        _model = model;
        _mapper = mapper;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var vm = await _model.GetIndexViewModelAsync(CancellationToken.None);
        return View(vm);
    }

    //todo: post
}