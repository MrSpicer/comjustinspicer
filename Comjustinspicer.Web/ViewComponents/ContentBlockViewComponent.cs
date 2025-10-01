using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.Data.ContentBlock;
using Comjustinspicer.Models.ContentBlock;
using System.Threading;

namespace Comjustinspicer.ViewComponents;

public class ContentBlockViewComponent : ViewComponent
{
	private readonly IContentBlockModel _model;

	public ContentBlockViewComponent(IContentBlockModel model)
	{
		_model = model ?? throw new ArgumentNullException(nameof(model));
	}

/// <summary>
/// Renders a content block by its ID.
/// </summary>
/// <param name="contentBlockID"></param>
/// <returns></returns>
	public async Task<IViewComponentResult> InvokeAsync(Guid contentBlockID)
	{
		if (contentBlockID == Guid.Empty)
		{
			return Content(string.Empty);
		}

		var model = await _model.FromIdAsync(contentBlockID, CancellationToken.None);
		if (model == null)
		{
			return View(new ContentBlockViewModel { Id = contentBlockID });
		}

		var vm = ContentBlockViewModel.FromDto(model);

		return View(vm);
	}
}
