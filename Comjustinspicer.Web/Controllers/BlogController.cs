using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using comjustinspicer.Models;
using comjustinspicer.Models.Blog;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;

namespace comjustinspicer.Controllers;



public class BlogController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<BlogController>();
    private readonly IBlogModel _blogModel;

    public BlogController(IBlogModel blogModel)
    {
        _blogModel = blogModel ?? throw new ArgumentNullException(nameof(blogModel));
    }

    public async Task<IActionResult> Index()
    {
        var vm = await _blogModel.GetIndexViewModelAsync();
        return View(vm);
    }
}
