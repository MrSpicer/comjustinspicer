using System.Threading;
using System.Threading.Tasks;

namespace Comjustinspicer.Models.ContentZone;

public class ContentZoneObject : IContentZoneObject
{
	public int Ordinal { get; set; } = 0;
	public Guid ZoneId { get; set; }

	public string ComponentName { get; set; }
	public Object ComponentProperties { get; set; }
}
