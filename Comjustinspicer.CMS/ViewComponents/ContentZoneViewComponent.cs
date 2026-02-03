using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.CMS.Models.ContentZone;

namespace Comjustinspicer.CMS.ViewComponents;

/// <summary>
/// ViewComponent that renders a content zone by name.
/// Content zones contain a list of other view components configured in the database.
/// </summary>
public class ContentZoneViewComponent : ViewComponent
{
	private readonly IContentZoneModel _model;

	public ContentZoneViewComponent(IContentZoneModel model)
	{
		_model = model ?? throw new ArgumentNullException(nameof(model));
	}

	/// <summary>
	/// Renders the content zone with the specified name.
	/// </summary>
	/// <param name="contentZoneName">The unique name of the content zone to render.</param>
	/// <returns>The rendered view containing all zone items.</returns>
	public async Task<IViewComponentResult> InvokeAsync(string contentZoneName)
	{
		if (string.IsNullOrWhiteSpace(contentZoneName))
			return Content(string.Empty);

		var vm = await _model.GetViewModelAsync(contentZoneName, HttpContext.RequestAborted);
		if (vm == null)
			return Content(string.Empty);

		return View(vm);
	}
}
