using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class MaintenanceRequest
    {
        public int RequestId { get; set; }
        public int SensorId { get; set; }
        public Sensor Sensor { get; set; }
        public int PriorityId { get; set; }
        public Priority Priority { get; set; }
        public string Description { get; set; }
        public DateTime? Deadline { get; set; }
        public int? AssignedTechnicianTo { get; set; }
        public User AssignedTechnician { get; set; }
        public int CreatedByStaffId { get; set; }
        public User CreatedByStaff { get; set; }
        public string Note { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? AssignedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
