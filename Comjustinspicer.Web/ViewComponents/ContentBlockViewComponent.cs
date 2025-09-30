using Microsoft.AspNetCore.Mvc;
using comjustinspicer.Data.ContentBlock;
using comjustinspicer.Models.ContentBlock;
using System.Threading;

namespace comjustinspicer.ViewComponents;

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

		var vm = ContentBlockViewModel.FromDto(model);

		return View(vm);
	}
}
