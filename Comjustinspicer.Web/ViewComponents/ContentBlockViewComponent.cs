using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.Data.ContentBlock;
using Comjustinspicer.Models.ContentBlock;
using System.Threading;

namespace Comjustinspicer.ViewComponents;

public class ContentBlockViewComponent : ViewComponent
{
	private readonly ContentBlockModel _model;

	public ContentBlockViewComponent(ContentBlockModel model)
	{
		_model = model ?? throw new ArgumentNullException(nameof(model));
	}

	// The view component expects a single argument named "ContentBlockID" of type Guid
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
