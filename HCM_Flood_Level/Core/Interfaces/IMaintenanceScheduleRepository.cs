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
        Task<IEnumerable<MaintenanceSchedule>> GetAllSchedulesAsync(EntityParam entityParam);
    }
}
