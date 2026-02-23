using System;

namespace Core.Entities
{
    public class FloodEvent
    {
        public int EventId { get; set; }
        public int SensorId { get; set; }
        public Sensor Sensor { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public float MaxWaterLevel { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


