using Core.DTOs;
using Core.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IAreaRepository : IGenericRepository<Area>
    {
        Task<IReadOnlyList<AreaDTO>> GetAreaSensorReadingsAsync(int? areaId = null, CancellationToken cancellationToken = default);
    }
}
