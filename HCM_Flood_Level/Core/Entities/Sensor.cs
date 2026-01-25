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
        public string SensorType { get; set; }
        public string SensorStatus { get; set; }
        public DateTime InstalledAt { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }
    }
}
