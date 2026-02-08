using System.Text.Json;
using Comjustinspicer.CMS.Data.Services;
using Comjustinspicer.CMS.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Comjustinspicer.CMS.Routing;

/// <summary>
/// Dynamic route value transformer that matches incoming request paths to
/// stored page routes and redirects to the registered controller.
/// </summary>
public class PageRouteTransformer : DynamicRouteValueTransformer
{
    private readonly IPageService _pageService;
    private readonly IPageControllerRegistry _registry;

    public PageRouteTransformer(IPageService pageService, IPageControllerRegistry registry)
    {
        _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public override async ValueTask<RouteValueDictionary> TransformAsync(
        HttpContext httpContext, RouteValueDictionary values)
    {
        var path = httpContext.Request.Path.Value ?? "/";

        // Normalize: lowercase, trim trailing slash (keep root)
        path = path.ToLowerInvariant();
        if (path.Length > 1 && path.EndsWith('/'))
            path = path.TrimEnd('/');

        var page = await _pageService.GetByRouteAsync(path);
        if (page == null)
            return null!;

        var controllerInfo = _registry.GetByName(page.ControllerName);
        if (controllerInfo == null)
            return null!;

        // Store page data and deserialized config in HttpContext.Items
        httpContext.Items["CMS:PageData"] = page;

        if (controllerInfo.ConfigurationType != null && !string.IsNullOrWhiteSpace(page.ConfigurationJson))
        {
            try
            {
                var config = JsonSerializer.Deserialize(page.ConfigurationJson, controllerInfo.ConfigurationType,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                httpContext.Items["CMS:PageConfig"] = config;
            }
            catch
            {
                // If deserialization fails, create a default instance
                httpContext.Items["CMS:PageConfig"] = Activator.CreateInstance(controllerInfo.ConfigurationType);
            }
        }

        return new RouteValueDictionary
        {
            { "controller", page.ControllerName },
            { "action", "Index" }
        };
    }
}
