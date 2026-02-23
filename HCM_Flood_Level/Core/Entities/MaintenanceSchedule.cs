using System;

namespace Core.Entities
{
    public class MaintenanceSchedule
    {
        public int ScheduleId { get; set; }
        public int SensorId { get; set; }
        public Sensor Sensor { get; set; }
        public string ScheduleType { get; set; } // Weekly | Monthly | Quarterly
        public string ScheduleMode { get; set; } // Manual | Auto
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int CreatedByStaffId { get; set; }
        public User CreatedByStaff { get; set; }
        public int AssignedTechnicianId { get; set; }
        public User AssignedTechnician { get; set; }
        public string Note { get; set; }
        public string Status { get; set; } = "Scheduled";
        public DateTime CreatedAt { get; set; }
    }
}


