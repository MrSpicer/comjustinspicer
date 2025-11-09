using Microsoft.AspNetCore.Mvc;

namespace Comjustinspicer.Controllers;



public class ArticleController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<ArticleController>();

    public ArticleController()
    {
    }

    public IActionResult Index()
    {
        return View();
    }
}
