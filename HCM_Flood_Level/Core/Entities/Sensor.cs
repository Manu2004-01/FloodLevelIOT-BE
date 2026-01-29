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
        public string SensorCode { get; set; }
        public string SensorName { get; set; }
        public string SensorType { get; set; }
        public string SensorStatus { get; set; }
        public double MinThreshold { get; set; }
        public double MaxThreshold { get; set; }
        public string ThresholdType { get; set; }
        public DateTime InstalledAt { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }

        public int? InstalledBy { get; set; }
        public Core.Entities.Staff InstalledByStaff { get; set; }
        public string Protocol { get; set; }
        public string Specification { get; set; }
        public double? WarningThreshold { get; set; }
        public double? DangerThreshold { get; set; }
        public int? MaxLevel { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
