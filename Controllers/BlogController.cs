using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using comjustinspicer.Models;
using comjustinspicer.Models.Blog;
using comjustinspicer.Data;
using comjustinspicer.Data.Models.Blog;

namespace comjustinspicer.Controllers;



public class BlogController : Controller
{
    private readonly ILogger<BlogController> _logger;


	//todo: inject this
	//private BlogContext _db = new BlogContext();

    private IEnumerable<Post> _posts;

    public BlogController(ILogger<BlogController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        //todo: remove. testing
		// _db.Add(new Post() { Id = Guid.NewGuid(), Title = $"this is a test {DateTime.Now}" });
		// _db.SaveChanges();
    
        var vm = new BlogViewModel();
        // vm.Posts = _db.Posts?.Select(p => new PostViewModel(p))?.ToList() ?? new List<PostViewModel>();

        return View("Index", vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
