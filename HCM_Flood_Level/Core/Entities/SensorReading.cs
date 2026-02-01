using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class SensorReading
    {
        public long ReadingId { get; set; }
        public int SensorId { get; set; }
        public string Status { get; set; }
        public double WaterLevel { get; set; }
        public int Battery { get; set; }
        public string SignalStrength { get; set; }
        public DateTime RecordAt { get; set; }
    }
}
