
using System.Threading;
using System.Threading.Tasks;
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IMapsService
    {
        Task<object> SearchAsync(MapsSearchDTO dto, CancellationToken ct = default);
        Task<object> GetPlaceDetailsAsync(string placeId, CancellationToken ct = default);
    }
}
