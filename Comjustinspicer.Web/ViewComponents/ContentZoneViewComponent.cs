using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.Data;
using Comjustinspicer.Models;
using System.Threading;
using Comjustinspicer.Models.ContentZone;

namespace Comjustinspicer.ViewComponents;

//this is a work in progress

public class ContentZoneViewComponent : ViewComponent
{
	private readonly IContentZoneModel _model;
	public ContentZoneViewComponent(IContentZoneModel model)
	{
		_model = model ?? throw new ArgumentNullException(nameof(model));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="contentZoneName"></param>
	/// <returns></returns>
	public async Task<IViewComponentResult> InvokeAsync(string contentZoneName)
	{
		var vm = await _model.GetViewModelAsync(contentZoneName, CancellationToken.None);
		return View(vm);
	}
}
