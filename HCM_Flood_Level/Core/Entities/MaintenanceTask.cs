using System;

namespace Core.Entities
{
    public class MaintenanceTask
    {
        public int TaskId { get; set; }

        public int RequestId { get; set; }
        public MaintenanceRequest Request { get; set; }

        public int SensorId { get; set; }
        public Sensor Sensor { get; set; }

        public int PriorityId { get; set; }
        public Priority Priority { get; set; }

        public string Description { get; set; }

        public int AssignedStaffId { get; set; }
        public Staff AssignedStaff { get; set; }

        public DateTime? Deadline { get; set; }

        public string Status { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


