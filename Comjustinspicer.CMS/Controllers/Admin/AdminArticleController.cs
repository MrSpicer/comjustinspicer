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
    private readonly IArticleListModel _articleListModel;

    public AdminArticleController(IArticleListModel articleListModel, IArticleModel articleModel)
    {
        _articleListModel = articleListModel ?? throw new ArgumentNullException(nameof(articleListModel));
        _postModel = articleModel ?? throw new ArgumentNullException(nameof(articleModel));
    }

    // ── ArticleList CRUD ──

    [HttpGet("Article")]
    public async Task<IActionResult> Index()
    {
        var vm = await _articleListModel.GetArticleListIndexAsync();
        return View(vm);
    }

    [HttpGet("Article/edit/{id?}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> ArticleListEdit(Guid? id)
    {
        var vm = await _articleListModel.GetArticleListUpsertAsync(id);
        if (vm == null && id != null) return NotFound();
        return View("ArticleListUpsert", vm!);
    }

    [HttpPost("Article/edit/{id?}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> ArticleListEdit(ArticleListUpsertViewModel model)
    {
        if (!ModelState.IsValid) return View("ArticleListUpsert", model);

        var result = await _articleListModel.SaveArticleListUpsertAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "An error occurred while saving.");
            return View("ArticleListUpsert", model);
        }

        return RedirectToAction("Index");
    }

    [HttpPost("Article/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArticleListDelete(Guid id)
    {
        var ok = await _articleListModel.DeleteArticleListAsync(id);
        if (!ok) TempData["Error"] = "Could not delete article list.";
        return RedirectToAction("Index");
    }

    // ── Articles within a list ──

    [HttpGet("Article/{listSlug}/articles")]
    public async Task<IActionResult> Articles(string listSlug)
    {
        var vm = await _articleListModel.GetArticlesForListBySlugAsync(listSlug);
        if (vm == null) return NotFound();
        return View("Articles", vm);
    }

    [HttpGet("Article/{listSlug}/articles/edit/{id?}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> PostEdit(string listSlug, Guid? id, string? returnUrl)
    {
        var list = await _articleListModel.GetArticlesForListBySlugAsync(listSlug);
        if (list == null) return NotFound();

        var vm = await _postModel.GetUpsertViewModelAsync(id, list.ArticleListId);
        if (vm == null && id != null) return NotFound();
        if (!string.IsNullOrWhiteSpace(returnUrl)) ViewData["ReturnUrl"] = returnUrl;
        ViewData["ArticleListSlug"] = listSlug;
        ViewData["ArticleListTitle"] = list.ArticleListTitle;
        return View("Upsert", vm!);
    }

    [HttpPost("Article/{listSlug}/articles/edit/{id?}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> PostEdit(string listSlug, ArticleUpsertViewModel model, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl)) ViewData["ReturnUrl"] = returnUrl;
            ViewData["ArticleListSlug"] = listSlug;
            return View("Upsert", model);
        }

        var result = await _postModel.SaveUpsertAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "An error occurred while saving the post.");
            if (!string.IsNullOrWhiteSpace(returnUrl)) ViewData["ReturnUrl"] = returnUrl;
            ViewData["ArticleListSlug"] = listSlug;
            return View("Upsert", model);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Articles", new { listSlug });
    }

    [HttpPost("Article/{listSlug}/articles/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string listSlug, Guid id)
    {
        var ok = await _postModel.DeleteAsync(id);
        if (!ok) TempData["Error"] = "Could not delete article.";
        return RedirectToAction("Articles", new { listSlug });
    }

    // ── API endpoints for entity pickers ──

    [HttpGet("article/api/list")]
    public async Task<IActionResult> ApiList()
    {
        var vm = await _articleListModel.GetIndexViewModelAsync();
        var result = vm.Articles.Select(p => new { id = p.Id, title = p.Title }).ToList();
        return Json(result);
    }

    [HttpGet("article/api/articlelists")]
    public async Task<IActionResult> ApiArticleLists()
    {
        var vm = await _articleListModel.GetArticleListIndexAsync();
        var result = vm.ArticleLists.Select(l => new { id = l.Id, title = l.Title }).ToList();
        return Json(result);
    }
}
