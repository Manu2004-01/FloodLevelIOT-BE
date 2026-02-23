using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Priority
    {
        public int PriorityId { get; set; }
        public string DisplayName { get; set; }
        // Navigation property
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; }
        public ICollection<MaintenanceTask> MaintenanceTasks { get; set; }
    }
}
