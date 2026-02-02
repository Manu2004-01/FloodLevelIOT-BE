using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class MaintenanceScheduleDTO
    {
        public string SensorName { get; set; }
        public string ScheduleType { get; set; }
        public string ScheduleMode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string AssignedStaff { get; set; }
        public string Status { get; set; }
        public string? Note { get; set; }

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
