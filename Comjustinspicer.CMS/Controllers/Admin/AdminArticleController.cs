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
    public async Task<IActionResult> PostEdit(Guid? id, string? returnUrl)
    {
        var vm = await _postModel.GetUpsertViewModelAsync(id);
        if (vm == null && id != null) return NotFound();
        if (!string.IsNullOrWhiteSpace(returnUrl)) ViewData["ReturnUrl"] = returnUrl;
        return View("Upsert", vm!);
    }

    [HttpPost("Article/post/edit/{id?}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> PostEdit(ArticleUpsertViewModel model, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl)) ViewData["ReturnUrl"] = returnUrl;
            return View("Upsert", model);
        }

        var result = await _postModel.SaveUpsertAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "An error occurred while saving the post.");
            if (!string.IsNullOrWhiteSpace(returnUrl)) ViewData["ReturnUrl"] = returnUrl;
            return View("Upsert", model);
        }

        // If a returnUrl was supplied and it's a local url, redirect there instead of Index
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index");
    }

    [HttpPost("Article/delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _postModel.DeleteAsync(id);
        if (!ok)
        {
            TempData["Error"] = "Could not delete article.";
        }

        return RedirectToAction("Index");
    }
}
