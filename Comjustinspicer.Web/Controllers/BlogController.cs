using Microsoft.AspNetCore.Mvc;

namespace Comjustinspicer.Controllers;

public class BlogController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<BlogController>();

    public BlogController()
    {
    }

    [Route("Blog")]
    [Route("Blog/{id:guid}")]
    public IActionResult Index(Guid? id = null)
    {
        ViewData["BlogId"] = id;
        return View();
    }
}
