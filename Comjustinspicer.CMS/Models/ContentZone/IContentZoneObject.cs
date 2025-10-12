using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Comjustinspicer.CMS.Models.ContentZone;

public interface IContentZoneObject
{
	int Ordinal { get; set; }
	Guid ZoneId { get; set; }

	string ComponentName { get; set; }
	Object ComponentProperties { get; set; }
}