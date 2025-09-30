using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.Models;

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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
