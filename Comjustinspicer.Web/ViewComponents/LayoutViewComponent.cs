using Microsoft.AspNetCore.Mvc;
using Comjustinspicer.Data;
using Comjustinspicer.Models;
using System.Threading;
using Comjustinspicer.Models.ContentZone;

namespace Comjustinspicer.ViewComponents;

//This is a work in progress to allow different layouts to be used for different pages.

public class LayoutViewComponent : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(string layoutName)
	{
		return View(layoutName);
	}
}