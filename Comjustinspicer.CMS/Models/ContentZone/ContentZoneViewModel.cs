using System.Threading;
using System.Threading.Tasks;

namespace Comjustinspicer.CMS.Models.ContentZone;

public class ContentZoneViewModel
{
	public string Name { get; set; } = string.Empty;

	public List<ContentZoneObject> ZoneObjects { get; set; } = new();
}
