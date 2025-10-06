using System.Threading;
using System.Threading.Tasks;

namespace Comjustinspicer.Models.ContentZone;

//probably move this to Data and use as dto
public class ContentZoneObject : IContentZoneObject
{
	public int Ordinal { get; set; } = 0;
	public Guid ZoneId { get; set; }

	public string ComponentName { get; set; } = string.Empty;
	public Object? ComponentProperties { get; set; } = default;
}
