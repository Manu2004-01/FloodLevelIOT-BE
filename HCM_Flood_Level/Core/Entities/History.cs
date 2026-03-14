using System;

namespace Core.Entities
{
    public class History
    {
        public int HistoryId { get; set; }
        public string LocationId { get; set; }
        public Location Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public float MaxWaterLevel { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
