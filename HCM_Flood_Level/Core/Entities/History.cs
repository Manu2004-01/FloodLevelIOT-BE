using System;

namespace Core.Entities
{
    public class History
    {
        public int HistoryId { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public float MaxWaterLevel { get; set; }
        public Severity Severity { get; set; } = Severity.Safe;
        public DateTime CreatedAt { get; set; }
    }
}
