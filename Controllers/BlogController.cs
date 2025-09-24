using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using comjustinspicer.Models;
using comjustinspicer.Models.Blog;
using comjustinspicer.Data;
using comjustinspicer.Data.Models.Blog;

namespace comjustinspicer.Controllers;



public class BlogController : Controller
{
    private readonly ILogger<BlogController> _logger;
    private readonly BlogContext _blogContext;

    public BlogController(ILogger<BlogController> logger, BlogContext blogContext)
    {
        _logger = logger;
        _blogContext = blogContext;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new BlogViewModel();

        var posts = await _blogContext.Posts
            .AsNoTracking()
            .OrderByDescending(p => p.Id)
            .Select(p => new PostViewModel(p))
            .ToListAsync();

        vm.Posts = posts;

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
