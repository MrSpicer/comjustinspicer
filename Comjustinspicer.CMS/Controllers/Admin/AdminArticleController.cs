using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.CMS.Models.Article;
using Microsoft.AspNetCore.Authorization;

namespace Comjustinspicer.CMS.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminArticleController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<AdminArticleController>();
    private readonly IArticleModel _postModel;
    private readonly IArticleListModel _ArticleListModel;

    public AdminArticleController(IArticleListModel ArticleListModel, IArticleModel articleModel)
    {
        _ArticleListModel = ArticleListModel ?? throw new ArgumentNullException(nameof(ArticleListModel));
        _postModel = articleModel ?? throw new ArgumentNullException(nameof(articleModel));
    }

    [HttpGet("Article")]
    public async Task<IActionResult> Index()
    {
        var vm = await _ArticleListModel.GetIndexViewModelAsync();
        return View(vm);
    }

    [HttpGet("Article/post/{id}")]
    public async Task<IActionResult> Index(Guid id)
    {
        var vm = await _postModel.GetPostViewModelAsync(id);
        if (vm == null) return NotFound();
        return View("Index", vm);
    }

    [HttpGet("Article/post/edit/{id?}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> PostEdit(Guid? id)
    {
        var vm = await _postModel.GetUpsertViewModelAsync(id);
        if (vm == null && id != null) return NotFound();
        return View("Upsert", vm!);
    }

    [HttpPost("Article/post/edit/{id?}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> PostEdit(PostUpsertViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Upsert", model);
        }

        var result = await _postModel.SaveUpsertAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "An error occurred while saving the post.");
            return View("Upsert", model);
        }

        return RedirectToAction("Index", "Article");
    }
}
