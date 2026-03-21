using Core.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IRouteAvoidFloodService
    {
        Task<RouteAvoidFloodResponseDTO> GetAvoidFloodRouteAsync(RouteAvoidFloodRequestDTO request, CancellationToken cancellationToken = default);
    }
}
