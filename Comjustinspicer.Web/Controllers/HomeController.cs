using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using comjustinspicer.Models;

namespace comjustinspicer.Controllers;

public class HomeController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<HomeController>();

    public HomeController()
    {
    }

    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
