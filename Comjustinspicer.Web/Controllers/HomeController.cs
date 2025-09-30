using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.Models;

namespace Comjustinspicer.Controllers;

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

}
