using Microsoft.AspNetCore.Mvc;

namespace Comjustinspicer.Controllers;

public class AboutController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<AboutController>();

    public AboutController()
    {
    }

    public IActionResult Index()
    {
        return View();
    }
}
