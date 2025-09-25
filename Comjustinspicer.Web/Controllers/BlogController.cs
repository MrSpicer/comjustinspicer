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
    private readonly IPostService _postService;

    public BlogController(ILogger<BlogController> logger, IPostService postService)
    {
        _logger = logger;
        _postService = postService;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new BlogViewModel();

        var posts = await _postService.GetAllAsync();
        vm.Posts = posts.Select(p => new PostViewModel(p)).ToList();

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
