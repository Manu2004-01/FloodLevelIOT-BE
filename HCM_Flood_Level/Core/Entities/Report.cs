using System;

namespace Core.Entities
{
    public class Report
    {
        public int ReportId { get; set; }
        public string LocationId { get; set; }
        public Location Location { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
