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
        public Sensor Sensor { get; set; }
        public string Status { get; set; } = "Offline";
        public float WaterLevelCm { get; set; }
        public int BatteryPercent { get; set; }
        public string SignalStrength { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
