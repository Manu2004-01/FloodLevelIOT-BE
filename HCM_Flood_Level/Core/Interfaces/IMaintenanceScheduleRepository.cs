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
    public interface IMaintenanceScheduleRepository : IGenericRepository<MaintenanceSchedule>
    {
        Task<bool> AddNewScheduleAsync(CreateMaintenanceScheduleDTO dto);
        Task<bool> AddAutoScheduleAsync(int sensorId);
        Task<IEnumerable<MaintenanceSchedule>> GetAllSchedulesAsync(EntityParam entityParam);
        Task<IEnumerable<MaintenanceSchedule>> GetSchedulesByTechnicianAsync(int technicianId, EntityParam entityParam);
        Task<bool> UpdateScheduleAsync(int id, UpdateMaintenanceScheduleDTO dto);
        Task<bool> UpdateScheduleStatusAsync(int id, string status);
        Task<bool> DeleteScheduleAsync(int id);
    }
}
