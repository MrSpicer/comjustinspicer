using Microsoft.AspNetCore.Hosting;

namespace Comjustinspicer.CMS.Services;

/// <summary>
/// Service for discovering available views for ViewComponents by scanning the filesystem.
/// Searches standard ASP.NET ViewComponent view locations.
/// </summary>
public sealed class ViewDiscoveryService : IViewDiscoveryService
{
    private readonly IWebHostEnvironment _env;
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<ViewDiscoveryService>();

    public ViewDiscoveryService(IWebHostEnvironment env)
    {
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    /// <summary>
    /// Gets a list of available view names for the specified ViewComponent.
    /// Scans:
    /// - Views/Shared/Components/{componentName}/*.cshtml
    /// - Areas/*/Views/Shared/Components/{componentName}/*.cshtml
    /// </summary>
    public IReadOnlyList<string> GetAvailableViews(string componentName)
    {
        if (string.IsNullOrWhiteSpace(componentName))
        {
            return Array.Empty<string>();
        }

        var views = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var contentRoot = _env.ContentRootPath;

        // Standard ViewComponent view locations
        var searchPaths = new[]
        {
            // Main project: Views/Shared/Components/{componentName}
            Path.Combine(contentRoot, "Views", "Shared", "Components", componentName),
            
            // Areas: Areas/*/Views/Shared/Components/{componentName}
            // We'll scan for area directories
        };

        // Scan main locations
        foreach (var searchPath in searchPaths)
        {
            ScanDirectory(searchPath, views);
        }

        // Scan areas
        var areasPath = Path.Combine(contentRoot, "Areas");
        if (Directory.Exists(areasPath))
        {
            foreach (var areaDir in Directory.GetDirectories(areasPath))
            {
                var areaComponentPath = Path.Combine(areaDir, "Views", "Shared", "Components", componentName);
                ScanDirectory(areaComponentPath, views);
            }
        }

        // Also scan class library projects (e.g., Comjustinspicer.CMS)
        // Look for sibling directories that might contain the CMS library
        var parentDir = Directory.GetParent(contentRoot);
        if (parentDir != null)
        {
            foreach (var siblingDir in Directory.GetDirectories(parentDir.FullName))
            {
                // Skip the current project
                if (siblingDir.Equals(contentRoot, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Check for Views/Shared/Components in sibling projects
                var siblingComponentPath = Path.Combine(siblingDir, "Views", "Shared", "Components", componentName);
                ScanDirectory(siblingComponentPath, views);

                // Check areas in sibling projects
                var siblingAreasPath = Path.Combine(siblingDir, "Areas");
                if (Directory.Exists(siblingAreasPath))
                {
                    foreach (var areaDir in Directory.GetDirectories(siblingAreasPath))
                    {
                        var areaComponentPath = Path.Combine(areaDir, "Views", "Shared", "Components", componentName);
                        ScanDirectory(areaComponentPath, views);
                    }
                }
            }
        }

        var result = views.OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToList();
        
        _logger.Debug("Discovered {Count} views for ViewComponent '{ComponentName}': {Views}", 
            result.Count, componentName, string.Join(", ", result));

        return result;
    }

    public IReadOnlyList<string> GetControllerViews(string controllerName)
    {
        if (string.IsNullOrWhiteSpace(controllerName))
            return Array.Empty<string>();

        var views = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var contentRoot = _env.ContentRootPath;

        ScanDirectory(Path.Combine(contentRoot, "Views", controllerName), views);

        var parentDir = Directory.GetParent(contentRoot);
        if (parentDir != null)
        {
            foreach (var siblingDir in Directory.GetDirectories(parentDir.FullName))
            {
                if (siblingDir.Equals(contentRoot, StringComparison.OrdinalIgnoreCase))
                    continue;
                ScanDirectory(Path.Combine(siblingDir, "Views", controllerName), views);
            }
        }

        var result = views.OrderBy(v => v, StringComparer.OrdinalIgnoreCase).ToList();
        _logger.Debug("Discovered {Count} controller views for '{ControllerName}': {Views}",
            result.Count, controllerName, string.Join(", ", result));
        return result;
    }

    private void ScanDirectory(string directoryPath, HashSet<string> views)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        try
        {
            foreach (var file in Directory.GetFiles(directoryPath, "*.cshtml", SearchOption.TopDirectoryOnly))
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                
                // Skip files that start with underscore (partials/layouts)
                if (!fileName.StartsWith("_"))
                {
                    views.Add(fileName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Error scanning directory {Directory} for ViewComponent views", directoryPath);
        }
    }
}
