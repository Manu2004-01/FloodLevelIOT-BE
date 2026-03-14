using Core.DTOs;
using Core.Entities;
using Core.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IManageSensorRepository : IGenericRepository<Sensor>
    {
        Task<IEnumerable<Sensor>> GetAllSensorsAsync(EntityParam param);
        Task<IEnumerable<SensorReading>> GetLatestReadingsForSensorIdsAsync(IEnumerable<int> sensorIds);
        Task<IEnumerable<int>> GetAllSensorIdsAsync();
        Task AddSensorReadingAsync(SensorReading reading);
        Task PruneSensorReadingsAsync(int sensorId, int maxEntries);
        Task<double?> GetMaxHistoryLevelForSensorAsync(int sensorId);
        Task AddHistoryAsync(History history);
        Task<bool> AddNewSensorAsync(CreateSensorDTO dto);
        Task<bool> LocationExistsAsync(string placeId);
        Task<bool> LocationHasSensorAsync(string placeId);
        Task<bool> UpdateSensorAsync(int id, UpdateSensorDTO dto);
        Task<bool> DeleteSensorAsync(int id);
    }
}
