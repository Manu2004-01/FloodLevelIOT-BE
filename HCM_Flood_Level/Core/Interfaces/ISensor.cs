using Core.DTOs.Sensor;
using Core.Entities;
using Core.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ISensor: IGenericRepository<Sensor>
    {
        Task<IEnumerable<Sensor>> GetAllSensorsAsync(EntityParam param);
        Task<int> CountAsync(string? search = null);
        Task<Sensor> GetSensorByIdAsync(int id);
        Task<bool> SensorCodeExistsAsync(string sensorCode, int? excludeId = null);
        Task<bool> AddNewSensorAsync(CreateSensorDTO dto);
        Task<bool> UpdateSensorAsync(int id, UpdateSensorDTO dto);
        Task<bool> UpdateLocationAsync(int sensorId, UpdateLocationDTO dto);
        Task<bool> UpdateThresholdAsync(int sensorId, UpdateThresholdDTO dto);
        Task<bool> DeleteSensorAsync(int id);
    }
}
