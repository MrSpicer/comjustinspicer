using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.Models.Blog;
using Microsoft.AspNetCore.Authorization;

namespace Comjustinspicer.Controllers;

public class BlogPostController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<BlogPostController>();
    private readonly IBlogPostModel _model;

    public BlogPostController(IBlogPostModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    [HttpGet("blog/post/{id}")]
    public async Task<IActionResult> Index(Guid id)
    {
        var vm = await _model.GetPostViewModelAsync(id);
        if (vm == null) return NotFound();
        return View("Index", vm);
    }

    [HttpGet("blog/post/edit/{id?}")]
	[Authorize(Roles = "Admin,Editor")]
	public async Task<IActionResult> Edit(Guid? id)
    {
        var vm = await _model.GetUpsertViewModelAsync(id);
        if (vm == null && id != null) return NotFound();
        return View("Upsert", vm!);
    }

    [HttpPost("blog/post/edit/{id?}")]
    [ValidateAntiForgeryToken]
	[Authorize(Roles = "Admin,Editor")]
	public async Task<IActionResult> Edit(PostUpsertViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Upsert", model);
        }

        var result = await _model.SaveUpsertAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "An error occurred while saving the post.");
            return View("Upsert", model);
        }

        return RedirectToAction("Index", "Blog");
    }
}
