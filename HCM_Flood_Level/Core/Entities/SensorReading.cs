using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class SensorReading
    {
        public int ReadingId { get; set; }
        public int SensorId { get; set; }
        public double WaterLevel { get; set; }
        public int Battery { get; set; }
        public int SignalStrength { get; set; }
        public DateTime RecordAt { get; set; }
    }
}
