using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using comjustinspicer.Models;

namespace comjustinspicer.Controllers;

public class ErrorController : Controller
{
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<ErrorController>();

    public IActionResult Index()
    {
        // For exceptions handled by UseExceptionHandler
        var exFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exFeature?.Error != null)
        {
            _logger.Error(exFeature.Error, "Unhandled exception occurred on path {Path}", exFeature.Path);
        }

        var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
        return View("Shared/Error", model);
    }

    [Route("Error/{statusCode}")]
    public IActionResult StatusCodeHandler(int statusCode)
    {
        // Log status codes like 404
        _logger.Warning("HTTP status code {StatusCode} returned for request {Path}", statusCode, HttpContext.Request.Path);
        var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
        return View("Shared/Error", model);
    }
}
