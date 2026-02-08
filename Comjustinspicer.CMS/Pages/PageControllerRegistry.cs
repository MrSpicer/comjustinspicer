using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Comjustinspicer.CMS.Attributes;
using Comjustinspicer.CMS.ContentZones;
using Microsoft.AspNetCore.Mvc;

namespace Comjustinspicer.CMS.Pages;

/// <summary>
/// Scans assemblies for Controllers decorated with <see cref="PageControllerAttribute"/>
/// and provides metadata for the admin UI.
/// </summary>
public class PageControllerRegistry : IPageControllerRegistry
{
    private readonly List<PageControllerInfo> _controllers;
    private readonly Dictionary<string, PageControllerInfo> _controllersByName;
    private readonly Dictionary<string, List<PageControllerInfo>> _controllersByCategory;

    public PageControllerRegistry(IEnumerable<Assembly> assemblies)
    {
        _controllers = new List<PageControllerInfo>();
        _controllersByName = new Dictionary<string, PageControllerInfo>(StringComparer.OrdinalIgnoreCase);
        _controllersByCategory = new Dictionary<string, List<PageControllerInfo>>(StringComparer.OrdinalIgnoreCase);

        ScanAssemblies(assemblies);
    }

    public PageControllerRegistry()
        : this(new[] { typeof(PageControllerRegistry).Assembly, Assembly.GetEntryAssembly()! }.Where(a => a != null).Distinct())
    {
    }

    private void ScanAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            try
            {
                ScanAssembly(assembly);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to scan assembly {assembly.FullName}: {ex.Message}");
            }
        }

        foreach (var categoryControllers in _controllersByCategory.Values)
        {
            categoryControllers.Sort((a, b) =>
            {
                var orderCompare = a.Order.CompareTo(b.Order);
                return orderCompare != 0 ? orderCompare : string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase);
            });
        }

        _controllers.Sort((a, b) =>
        {
            var catCompare = string.Compare(a.Category, b.Category, StringComparison.OrdinalIgnoreCase);
            if (catCompare != 0) return catCompare;
            var orderCompare = a.Order.CompareTo(b.Order);
            return orderCompare != 0 ? orderCompare : string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase);
        });
    }

    private void ScanAssembly(Assembly assembly)
    {
        var controllerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                && typeof(Controller).IsAssignableFrom(t)
                && !typeof(ViewComponent).IsAssignableFrom(t));

        foreach (var type in controllerTypes)
        {
            var attribute = type.GetCustomAttribute<PageControllerAttribute>();
            if (attribute == null)
                continue;

            var controllerName = GetControllerName(type);
            var info = BuildControllerInfo(type, attribute, controllerName);

            _controllers.Add(info);
            _controllersByName[controllerName] = info;

            if (!_controllersByCategory.TryGetValue(info.Category, out var categoryList))
            {
                categoryList = new List<PageControllerInfo>();
                _controllersByCategory[info.Category] = categoryList;
            }
            categoryList.Add(info);
        }
    }

    private static string GetControllerName(Type type)
    {
        const string suffix = "Controller";
        var name = type.Name;
        return name.EndsWith(suffix, StringComparison.Ordinal)
            ? name[..^suffix.Length]
            : name;
    }

    private static PageControllerInfo BuildControllerInfo(Type type, PageControllerAttribute attribute, string controllerName)
    {
        var info = new PageControllerInfo
        {
            Name = controllerName,
            DisplayName = string.IsNullOrEmpty(attribute.DisplayName) ? InsertSpaces(controllerName) : attribute.DisplayName,
            Description = attribute.Description,
            Category = attribute.Category,
            IconClass = attribute.IconClass,
            Order = attribute.Order,
            ControllerType = type,
            ConfigurationType = attribute.ConfigurationType
        };

        if (attribute.ConfigurationType != null)
        {
            info.Properties = BuildPropertyInfos(attribute.ConfigurationType);
        }

        return info;
    }

    private static List<ContentZonePropertyInfo> BuildPropertyInfos(Type configurationType)
    {
        var properties = new List<ContentZonePropertyInfo>();

        foreach (var prop in configurationType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            var attr = prop.GetCustomAttribute<ContentZonePropertyAttribute>();
            var requiredAttr = prop.GetCustomAttribute<RequiredAttribute>();
            var rangeAttr = prop.GetCustomAttribute<RangeAttribute>();
            var stringLengthAttr = prop.GetCustomAttribute<StringLengthAttribute>();
            var regexAttr = prop.GetCustomAttribute<RegularExpressionAttribute>();

            var propInfo = new ContentZonePropertyInfo
            {
                Name = prop.Name,
                Label = attr?.Label ?? InsertSpaces(prop.Name),
                HelpText = attr?.HelpText ?? string.Empty,
                Placeholder = attr?.Placeholder ?? string.Empty,
                EditorType = attr?.EditorType ?? InferEditorType(prop.PropertyType),
                PropertyType = prop.PropertyType,
                Order = attr?.Order ?? 0,
                CssClass = attr?.CssClass ?? string.Empty,
                GroupWithNext = attr?.GroupWithNext ?? false,
                Group = attr?.Group ?? string.Empty,
                IsRequired = attr?.IsRequired == true || requiredAttr != null,
                EntityType = attr?.EntityType ?? string.Empty,
                ViewComponentName = attr?.ViewComponentName ?? string.Empty,
                Min = GetMinValue(attr, rangeAttr),
                Max = GetMaxValue(attr, rangeAttr),
                MaxLength = GetMaxLengthValue(attr, stringLengthAttr),
                Pattern = attr?.Pattern ?? regexAttr?.Pattern ?? string.Empty,
                PatternErrorMessage = attr?.PatternErrorMessage ?? regexAttr?.ErrorMessage ?? string.Empty,
                DefaultValue = GetDefaultValue(prop.PropertyType)
            };

            if (!string.IsNullOrEmpty(attr?.DropdownOptions))
            {
                propInfo.DropdownOptions = ParseDropdownOptions(attr.DropdownOptions);
            }

            properties.Add(propInfo);
        }

        properties.Sort((a, b) =>
        {
            var orderCompare = a.Order.CompareTo(b.Order);
            return orderCompare != 0 ? orderCompare : string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        });

        return properties;
    }

    private static EditorType InferEditorType(Type propertyType)
    {
        var underlying = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (underlying == typeof(Guid))
            return EditorType.Guid;
        if (underlying == typeof(bool))
            return EditorType.Checkbox;
        if (underlying == typeof(int) || underlying == typeof(long) || underlying == typeof(short) ||
            underlying == typeof(decimal) || underlying == typeof(double) || underlying == typeof(float))
            return EditorType.Number;
        if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset))
            return EditorType.DateTime;
        if (underlying == typeof(DateOnly))
            return EditorType.Date;
        if (underlying.IsEnum)
            return EditorType.Dropdown;

        return EditorType.Text;
    }

    private static object? GetDefaultValue(Type propertyType)
    {
        if (propertyType.IsValueType)
            return Activator.CreateInstance(propertyType);
        return null;
    }

    private static double? GetMinValue(ContentZonePropertyAttribute? attr, RangeAttribute? rangeAttr)
    {
        if (attr != null && !double.IsNaN(attr.Min))
            return attr.Min;
        if (rangeAttr?.Minimum != null && double.TryParse(rangeAttr.Minimum.ToString(), out var min))
            return min;
        return null;
    }

    private static double? GetMaxValue(ContentZonePropertyAttribute? attr, RangeAttribute? rangeAttr)
    {
        if (attr != null && !double.IsNaN(attr.Max))
            return attr.Max;
        if (rangeAttr?.Maximum != null && double.TryParse(rangeAttr.Maximum.ToString(), out var max))
            return max;
        return null;
    }

    private static int? GetMaxLengthValue(ContentZonePropertyAttribute? attr, StringLengthAttribute? stringLengthAttr)
    {
        if (attr != null && attr.MaxLength >= 0)
            return attr.MaxLength;
        return stringLengthAttr?.MaximumLength;
    }

    private static Dictionary<string, string> ParseDropdownOptions(string options)
    {
        var result = new Dictionary<string, string>();
        var pairs = options.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var parts = pair.Split(':', 2);
            var value = parts[0].Trim();
            var label = parts.Length > 1 ? parts[1].Trim() : value;
            result[value] = label;
        }

        return result;
    }

    private static string InsertSpaces(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return Regex.Replace(input, "(?<!^)([A-Z])", " $1");
    }

    #region IPageControllerRegistry Implementation

    public IReadOnlyList<PageControllerInfo> GetAllControllers() => _controllers.AsReadOnly();

    public PageControllerInfo? GetByName(string controllerName)
    {
        if (string.IsNullOrEmpty(controllerName))
            return null;

        _controllersByName.TryGetValue(controllerName, out var info);
        return info;
    }

    public IReadOnlyList<string> GetCategories()
    {
        return _controllersByCategory.Keys.OrderBy(c => c).ToList().AsReadOnly();
    }

    public IReadOnlyList<PageControllerInfo> GetByCategory(string category)
    {
        if (string.IsNullOrEmpty(category))
            return Array.Empty<PageControllerInfo>();

        return _controllersByCategory.TryGetValue(category, out var list)
            ? list.AsReadOnly()
            : Array.Empty<PageControllerInfo>();
    }

    public object? CreateDefaultConfiguration(string controllerName)
    {
        var info = GetByName(controllerName);
        if (info?.ConfigurationType == null)
            return null;

        try
        {
            return Activator.CreateInstance(info.ConfigurationType);
        }
        catch
        {
            return null;
        }
    }

    public IReadOnlyList<string> ValidateConfiguration(string controllerName, object configuration)
    {
        var errors = new List<string>();
        var info = GetByName(controllerName);

        if (info == null)
        {
            errors.Add($"Unknown controller: {controllerName}");
            return errors;
        }

        if (info.ConfigurationType == null)
            return errors;

        object? configObj = configuration;
        if (configuration is string jsonString)
        {
            try
            {
                configObj = JsonSerializer.Deserialize(jsonString, info.ConfigurationType, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                errors.Add($"Invalid JSON: {ex.Message}");
                return errors;
            }
        }

        if (configObj == null)
        {
            errors.Add("Configuration is required.");
            return errors;
        }

        foreach (var propInfo in info.Properties)
        {
            var prop = info.ConfigurationType.GetProperty(propInfo.Name);
            if (prop == null)
                continue;

            var value = prop.GetValue(configObj);

            if (propInfo.IsRequired)
            {
                if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)) ||
                    (value is Guid g && g == Guid.Empty))
                {
                    errors.Add($"{propInfo.Label} is required.");
                }
            }

            if (value != null && (propInfo.Min.HasValue || propInfo.Max.HasValue))
            {
                if (double.TryParse(value.ToString(), out var numValue))
                {
                    if (propInfo.Min.HasValue && numValue < propInfo.Min.Value)
                        errors.Add($"{propInfo.Label} must be at least {propInfo.Min.Value}.");
                    if (propInfo.Max.HasValue && numValue > propInfo.Max.Value)
                        errors.Add($"{propInfo.Label} must be at most {propInfo.Max.Value}.");
                }
            }

            if (value is string strValue && propInfo.MaxLength.HasValue && strValue.Length > propInfo.MaxLength.Value)
            {
                errors.Add($"{propInfo.Label} must not exceed {propInfo.MaxLength.Value} characters.");
            }

            if (value is string patternValue && !string.IsNullOrEmpty(propInfo.Pattern))
            {
                if (!Regex.IsMatch(patternValue, propInfo.Pattern))
                {
                    errors.Add(!string.IsNullOrEmpty(propInfo.PatternErrorMessage)
                        ? propInfo.PatternErrorMessage
                        : $"{propInfo.Label} has an invalid format.");
                }
            }
        }

        return errors;
    }

    #endregion
}
