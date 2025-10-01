using System.Threading;
using System.Threading.Tasks;

namespace Comjustinspicer.Models.ContentZone;

/// <summary>
/// </summary>
public interface IContentZoneModel
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentZoneName">The content zone identifier/name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An object representing the view model for the content zone.</returns>
    Task<object?> GetViewModelAsync(string contentZoneName, CancellationToken ct = default);
}
