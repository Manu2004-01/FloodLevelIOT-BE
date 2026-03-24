using System.Threading;
using System.Threading.Tasks;
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IOpenWeatherService
    {
        Task<CurrentWeatherDTO?> GetCurrentByCoordinatesAsync(double lat, double lon, CancellationToken cancellationToken = default);
    }
}
