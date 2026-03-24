using System;

namespace Core.Entities
{
    public class Report
    {
        public int ReportId { get; set; }
        public string? Description { get; set; }
        public string? ForecastRiskLevel { get; set; }
        public string? ForecastDataJson { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
