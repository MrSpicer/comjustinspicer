using Comjustinspicer.CMS.Attributes;

namespace Comjustinspicer.CMS.Models.Article;

/// <summary>
/// Configuration model for the Article ViewComponent when used within a content zone.
/// Defines the properties that can be configured in the admin UI.
/// </summary>
public class ArticleContentZoneConfiguration
{

    /// <summary>
    /// Gets or sets the ID of a specific article to display.
    /// If set, displays a single article. If empty, displays the article list.
    /// </summary>
    [FormProperty(
        Label = "Article",
        HelpText = "Select a specific article to display, or leave empty to show the article list.",
        EditorType = EditorType.Guid,
        EntityType = "Article",
        Order = 1
    )]
    public Guid? Id { get; set; }

//todo: this has problems. it works, but it lists all views which isn't actually what we want for this widget. I selected list and the page broke because it's not a list. which it probably should be? or at least should be if you select multiple which you can do right now
        /// <summary>
    /// Gets or sets the name of the view to use for rendering.
    /// Common values: "List" (list), "Post" (single article), "Summary".
    /// </summary>
    [FormProperty(
        Label = "View Name",
        HelpText = "The view template to use. Leave empty for default behavior.",
        Placeholder = "e.g., Post, List, Summary",
        EditorType = EditorType.ViewPicker,
        ViewComponentName = "Article",
        Order = 2
    )]
    public string? ViewName { get; set; }

//todo: probably remove this and add an invoke that an article object so the article view component can pass it in directly for the single article view
    /// <summary>
    /// Gets or sets an article view model to render directly.
    /// This is typically set programmatically, not through the admin UI.
    /// </summary>
    public ArticleViewModel? Article { get; set; }
}
