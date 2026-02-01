using Core.DTOs.Admin;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Admin
{
    public interface IMaintenanceScheduleRepository : IGenericRepository<MaintenanceSchedule>
    {
        Task<bool> AddNewScheduleAsync(CreateMaintenanceScheduleDTO dto);
    }
}
