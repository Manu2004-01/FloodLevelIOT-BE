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

        // Deadline for handling the request (nullable to support optional deadlines)
        public DateTime? Deadline { get; set; }

        public int AssignTo { get; set; }
        public Core.Entities.Staff AssignedUser { get; set; }

        // User who created the request
        public int CreatedBy { get; set; }
        public Core.Entities.Staff CreatedByUser { get; set; }

        public string Note { get; set; }
        public string Status { get; set; }  
        public DateTime CreatedAt { get; set; }
    }
}
