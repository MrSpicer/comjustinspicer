namespace Comjustinspicer.CMS.Services;

/// <summary>
/// Service for discovering available views for ViewComponents.
/// Scans the ASP.NET view search paths to find .cshtml files in ViewComponent directories.
/// </summary>
public interface IViewComponentViewDiscoveryService
{
    /// <summary>
    /// Gets a list of available view names for the specified ViewComponent.
    /// </summary>
    /// <param name="componentName">The name of the ViewComponent (without "ViewComponent" suffix).</param>
    /// <returns>
    /// A list of view names (without .cshtml extension) found in the ViewComponent's view directories.
    /// Returns an empty list if no views are found or the component directory doesn't exist.
    /// </returns>
    IReadOnlyList<string> GetAvailableViews(string componentName);

    /// <summary>
    /// Gets a list of available view names for the specified page controller.
    /// Scans Views/{controllerName}/ in the main project and sibling projects.
    /// </summary>
    /// <param name="controllerName">The name of the page controller.</param>
    /// <returns>
    /// A list of view names (without .cshtml extension) found in the controller's view directory.
    /// Returns an empty list if no views are found or the directory doesn't exist.
    /// </returns>
    IReadOnlyList<string> GetControllerViews(string controllerName);
}
