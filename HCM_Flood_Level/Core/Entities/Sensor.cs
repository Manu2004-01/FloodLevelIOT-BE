using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Sensor
    {
        public int SensorId { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public int TechnicianId { get; set; }
        public User Technician { get; set; }
        public string SensorCode { get; set; }
        public string SensorName { get; set; }
        public string SensorType { get; set; }
        public string Protocol { get; set; }
        public string Specification { get; set; }
        public DateTime? InstalledAt { get; set; }
        public float? WarningThreshold { get; set; }
        public float? DangerThreshold { get; set; }
        public int? MaxLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        // Navigation properties
        public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; }
        public ICollection<MaintenanceSchedule> MaintenanceSchedules { get; set; }
        public ICollection<MaintenanceTask> MaintenanceTasks { get; set; }
        public ICollection<SensorReading> SensorReadings { get; set; }
    }
}
