using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.CMS.Data;
using Comjustinspicer.Models;
using System.Threading;
using Comjustinspicer.CMS.Models.ContentZone;

namespace Comjustinspicer.CMS.ViewComponents;

//This is a work in progress to allow different layouts to be used for different pages.

public class LayoutViewComponent : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(string layoutName)
	{
		await Task.Delay(0);
		return View(layoutName);
	}
}