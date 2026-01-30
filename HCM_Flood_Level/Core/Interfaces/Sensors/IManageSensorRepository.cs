using Core.DTOs.Sensor;
using Core.Entities;
using Core.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Sensors
{
    public interface IManageSensorRepository : IGenericRepository<Sensor>
    {
        Task<IEnumerable<Sensor>> GetAllSensorsAsync(EntityParam param);
        Task<bool> AddNewSensorAsync(CreateSensorDTO dto);
        Task<bool> UpdateSensorAsync(int id, UpdateSensorDTO dto);
        Task<bool> DeleteSensorAsync(int id);
    }
}
