using System.Threading;
using System.Threading.Tasks;

namespace Comjustinspicer.Models.ContentZone;

public class ContentZoneModel : IContentZoneModel
{
    public async Task<object?> GetViewModelAsync(string contentZoneName, CancellationToken ct = default)
	{
		// Simulate asynchronous data retrieval
		await Task.Delay(0, ct);
		return new { Name = contentZoneName };
	}
}
