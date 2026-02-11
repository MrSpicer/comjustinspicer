using System.Text.Json;
using Comjustinspicer.CMS.Data.Services;
using Comjustinspicer.CMS.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Comjustinspicer.CMS.Routing;

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

        // Try exact match first
        var page = await _pageService.GetByRouteAsync(path);
        string? subRoute = null;

        // If no exact match, try progressively shorter paths for sub-route matching
        if (page == null && path != "/")
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int i = segments.Length - 1; i >= 1; i--)
            {
                var parentPath = "/" + string.Join('/', segments[..i]);
                page = await _pageService.GetByRouteAsync(parentPath);
                if (page != null)
                {
                    subRoute = string.Join('/', segments[i..]);
                    break;
                }
            }

            // If still no match, try root page as parent
            if (page == null)
            {
                page = await _pageService.GetByRouteAsync("/");
                if (page != null)
                {
                    subRoute = string.Join('/', segments);
                }
            }
        }

        if (page == null)
            return null!;

        var controllerInfo = _registry.GetByName(page.ControllerName);
        if (controllerInfo == null)
            return null!;

        // Store page data and deserialized config in HttpContext.Items
        httpContext.Items["CMS:PageData"] = page;

        if (subRoute != null)
            httpContext.Items["CMS:SubRoute"] = subRoute;

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
