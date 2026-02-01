using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.Admin
{
    public class MaintenanceScheduleDTO
    {
    }

    public class CreateMaintenanceScheduleDTO
    {
        public int SensorId { get; set; }
        public string ScheduleType { get; set; }
        public string ScheduleMode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? AssignedStaffId { get; set; }
        public string? Note { get; set; }
    }
}
