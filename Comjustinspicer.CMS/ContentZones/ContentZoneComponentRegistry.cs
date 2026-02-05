using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Comjustinspicer.CMS.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Comjustinspicer.CMS.ContentZones;

/// <summary>
/// Scans assemblies for ViewComponents decorated with <see cref="ContentZoneComponentAttribute"/>
/// and provides metadata for the admin UI.
/// </summary>
public class ContentZoneComponentRegistry : IContentZoneComponentRegistry
{
    private readonly List<ContentZoneComponentInfo> _components;
    private readonly Dictionary<string, ContentZoneComponentInfo> _componentsByName;
    private readonly Dictionary<string, List<ContentZoneComponentInfo>> _componentsByCategory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentZoneComponentRegistry"/> class.
    /// Scans the provided assemblies for registered ViewComponents.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for ViewComponents.</param>
    public ContentZoneComponentRegistry(IEnumerable<Assembly> assemblies)
    {
        _components = new List<ContentZoneComponentInfo>();
        _componentsByName = new Dictionary<string, ContentZoneComponentInfo>(StringComparer.OrdinalIgnoreCase);
        _componentsByCategory = new Dictionary<string, List<ContentZoneComponentInfo>>(StringComparer.OrdinalIgnoreCase);

        ScanAssemblies(assemblies);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentZoneComponentRegistry"/> class.
    /// Scans the calling assembly and the CMS assembly for registered ViewComponents.
    /// </summary>
    public ContentZoneComponentRegistry()
        : this(new[] { typeof(ContentZoneComponentRegistry).Assembly, Assembly.GetEntryAssembly()! }.Where(a => a != null).Distinct())
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
                // Log but don't fail - some assemblies may not be loadable
                System.Diagnostics.Debug.WriteLine($"Failed to scan assembly {assembly.FullName}: {ex.Message}");
            }
        }

        // Sort components within each category by Order, then DisplayName
        foreach (var categoryComponents in _componentsByCategory.Values)
        {
            categoryComponents.Sort((a, b) =>
            {
                var orderCompare = a.Order.CompareTo(b.Order);
                return orderCompare != 0 ? orderCompare : string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase);
            });
        }

        // Sort main list
        _components.Sort((a, b) =>
        {
            var catCompare = string.Compare(a.Category, b.Category, StringComparison.OrdinalIgnoreCase);
            if (catCompare != 0) return catCompare;
            var orderCompare = a.Order.CompareTo(b.Order);
            return orderCompare != 0 ? orderCompare : string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase);
        });
    }

    private void ScanAssembly(Assembly assembly)
    {
        var viewComponentTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ViewComponent).IsAssignableFrom(t));

        foreach (var type in viewComponentTypes)
        {
            var attribute = type.GetCustomAttribute<ContentZoneComponentAttribute>();
            if (attribute == null)
                continue;

            var componentName = GetComponentName(type);
            var info = BuildComponentInfo(type, attribute, componentName);

            _components.Add(info);
            _componentsByName[componentName] = info;

            if (!_componentsByCategory.TryGetValue(info.Category, out var categoryList))
            {
                categoryList = new List<ContentZoneComponentInfo>();
                _componentsByCategory[info.Category] = categoryList;
            }
            categoryList.Add(info);
        }
    }

    private static string GetComponentName(Type type)
    {
        // ViewComponent convention: remove "ViewComponent" suffix
        const string suffix = "ViewComponent";
        var name = type.Name;
        return name.EndsWith(suffix, StringComparison.Ordinal)
            ? name[..^suffix.Length]
            : name;
    }

    private static ContentZoneComponentInfo BuildComponentInfo(Type type, ContentZoneComponentAttribute attribute, string componentName)
    {
        var info = new ContentZoneComponentInfo
        {
            Name = componentName,
            DisplayName = string.IsNullOrEmpty(attribute.DisplayName) ? InsertSpaces(componentName) : attribute.DisplayName,
            Description = attribute.Description,
            Category = attribute.Category,
            IconClass = attribute.IconClass,
            Order = attribute.Order,
            ViewComponentType = type,
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

            // Parse dropdown options
            if (!string.IsNullOrEmpty(attr?.DropdownOptions))
            {
                propInfo.DropdownOptions = ParseDropdownOptions(attr.DropdownOptions);
            }

            properties.Add(propInfo);
        }

        // Sort by Order, then by name
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
        // Check attribute first (NaN means not set)
        if (attr != null && !double.IsNaN(attr.Min))
            return attr.Min;
        // Fall back to RangeAttribute
        if (rangeAttr?.Minimum != null && double.TryParse(rangeAttr.Minimum.ToString(), out var min))
            return min;
        return null;
    }

    private static double? GetMaxValue(ContentZonePropertyAttribute? attr, RangeAttribute? rangeAttr)
    {
        // Check attribute first (NaN means not set)
        if (attr != null && !double.IsNaN(attr.Max))
            return attr.Max;
        // Fall back to RangeAttribute
        if (rangeAttr?.Maximum != null && double.TryParse(rangeAttr.Maximum.ToString(), out var max))
            return max;
        return null;
    }

    private static int? GetMaxLengthValue(ContentZonePropertyAttribute? attr, StringLengthAttribute? stringLengthAttr)
    {
        // Check attribute first (-1 means not set)
        if (attr != null && attr.MaxLength >= 0)
            return attr.MaxLength;
        // Fall back to StringLengthAttribute
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

        // Insert space before each uppercase letter (except the first)
        return Regex.Replace(input, "(?<!^)([A-Z])", " $1");
    }

    #region IContentZoneComponentRegistry Implementation

    /// <inheritdoc />
    public IReadOnlyList<ContentZoneComponentInfo> GetAllComponents() => _components.AsReadOnly();

    /// <inheritdoc />
    public ContentZoneComponentInfo? GetByName(string componentName)
    {
        if (string.IsNullOrEmpty(componentName))
            return null;

        _componentsByName.TryGetValue(componentName, out var info);
        return info;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetCategories()
    {
        return _componentsByCategory.Keys.OrderBy(c => c).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<ContentZoneComponentInfo> GetByCategory(string category)
    {
        if (string.IsNullOrEmpty(category))
            return Array.Empty<ContentZoneComponentInfo>();

        return _componentsByCategory.TryGetValue(category, out var list)
            ? list.AsReadOnly()
            : Array.Empty<ContentZoneComponentInfo>();
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IReadOnlyList<ContentZoneComponentInfo>> GetComponentsByCategory()
    {
        return _componentsByCategory.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<ContentZoneComponentInfo>)kvp.Value.AsReadOnly()
        );
    }

    /// <inheritdoc />
    public object? CreateDefaultConfiguration(string componentName)
    {
        var info = GetByName(componentName);
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

    /// <inheritdoc />
    public IReadOnlyList<string> ValidateConfiguration(string componentName, object configuration)
    {
        var errors = new List<string>();
        var info = GetByName(componentName);

        if (info == null)
        {
            errors.Add($"Unknown component: {componentName}");
            return errors;
        }

        if (info.ConfigurationType == null)
            return errors; // No configuration required

        // If configuration is a string, try to deserialize it
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

        // Validate each property
        foreach (var propInfo in info.Properties)
        {
            var prop = info.ConfigurationType.GetProperty(propInfo.Name);
            if (prop == null)
                continue;

            var value = prop.GetValue(configObj);

            // Required check
            if (propInfo.IsRequired)
            {
                if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)) ||
                    (value is Guid g && g == Guid.Empty))
                {
                    errors.Add($"{propInfo.Label} is required.");
                }
            }

            // Range check for numeric types
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

            // MaxLength check for strings
            if (value is string strValue && propInfo.MaxLength.HasValue && strValue.Length > propInfo.MaxLength.Value)
            {
                errors.Add($"{propInfo.Label} must not exceed {propInfo.MaxLength.Value} characters.");
            }

            // Pattern check
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
