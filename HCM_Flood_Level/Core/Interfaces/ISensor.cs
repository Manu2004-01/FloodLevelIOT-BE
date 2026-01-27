using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ISensor: IGenericRepository<Sensor>
    {
        Task<IEnumerable<Sensor>> GetAllWithDetailsAsync();
        Task<Sensor> GetByIdWithDetailsAsync(int id);
        Task<bool> SensorCodeExistsAsync(string sensorCode, int? excludeId = null);
        Task<bool> UpdateLocationAsync(int sensorId, double latitude, double longitude, string address);
        Task<bool> UpdateThresholdAsync(int sensorId, double? minThreshold, double? maxThreshold, string thresholdType);
    }
}
