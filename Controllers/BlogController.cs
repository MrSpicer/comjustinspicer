using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using comjustinspicer.Models;
using comjustinspice.Data;
using comjustinspice.Data.Models.Blog;

namespace comjustinspicer.Controllers;



public class BlogController : Controller
{
    private readonly ILogger<BlogController> _logger;


	//todo: inject this
	private BlogContext _db = new BlogContext();

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
    
        _posts = _db.Posts.Take(5).ToList();
       Console.WriteLine(String.Join(',', _posts.Select(p => p.Title)));

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
