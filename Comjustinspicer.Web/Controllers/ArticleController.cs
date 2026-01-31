using Microsoft.AspNetCore.Mvc;

namespace Comjustinspicer.Controllers;

public class ArticleController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<ArticleController>();

    public ArticleController()
    {
    }

    [Route("Article")]
    [Route("Article/{id:guid}")]
    public IActionResult Index(Guid? id = null)
    {
        ViewData["ArticleId"] = id;
        return View();
    }
}
