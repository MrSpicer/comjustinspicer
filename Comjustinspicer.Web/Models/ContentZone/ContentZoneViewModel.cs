using System.Threading;
using System.Threading.Tasks;

namespace Comjustinspicer.Models.ContentZone;

public class ContentZoneViewModel
{
	public string Name { get; set; } = string.Empty;

	public List<ContentZoneObject> ZoneObjects { get; set; } = new();
}
