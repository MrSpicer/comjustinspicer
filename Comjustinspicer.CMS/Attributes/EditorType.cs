namespace Comjustinspicer.CMS.Attributes;

/// <summary>
/// Defines the type of editor to use for a content zone configuration property.
/// </summary>
public enum EditorType
{
    /// <summary>
    /// Single-line text input.
    /// </summary>
    Text,

    /// <summary>
    /// Multi-line text area.
    /// </summary>
    TextArea,

    /// <summary>
    /// Rich text / HTML editor.
    /// </summary>
    RichText,

    /// <summary>
    /// Numeric input.
    /// </summary>
    Number,

    /// <summary>
    /// Checkbox for boolean values.
    /// </summary>
    Checkbox,

    /// <summary>
    /// GUID input with optional entity picker.
    /// </summary>
    Guid,

    /// <summary>
    /// Dropdown select from predefined options.
    /// </summary>
    Dropdown,

    /// <summary>
    /// Date picker.
    /// </summary>
    Date,

    /// <summary>
    /// Date and time picker.
    /// </summary>
    DateTime,

    /// <summary>
    /// Color picker.
    /// </summary>
    Color,

    /// <summary>
    /// URL input with validation.
    /// </summary>
    Url,

    /// <summary>
    /// Email input with validation.
    /// </summary>
    Email,

    /// <summary>
    /// Dropdown populated with available views for a ViewComponent.
    /// Requires ViewComponentName to be specified in the ContentZonePropertyAttribute.
    /// </summary>
    ViewPicker,

    /// <summary>
    /// Hidden field (not displayed in form but included in configuration).
    /// </summary>
    Hidden
}
