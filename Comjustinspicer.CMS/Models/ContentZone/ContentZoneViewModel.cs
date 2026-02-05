using System.Threading;
using System.Threading.Tasks;

namespace Comjustinspicer.CMS.Models.ContentZone;

public class ContentZoneViewModel
{
	/// <summary>
	/// The database ID of the content zone (Guid.Empty if the zone doesn't exist yet).
	/// Used internally for saving items but never displayed to users.
	/// </summary>
	public Guid Id { get; set; } = Guid.Empty;

	/// <summary>
	/// The unique path/name that identifies this content zone.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The zone objects (items) to render in this zone.
	/// </summary>
	public List<ContentZoneObject> ZoneObjects { get; set; } = new();

	/// <summary>
	/// Indicates whether the current user can edit this zone.
	/// </summary>
	public bool CanEdit { get; set; } = false;
}
