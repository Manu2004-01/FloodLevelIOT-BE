using System;
using System.Collections.Generic;

namespace Core.DTOs
{
    public class FloodForecastRequestDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusKm { get; set; } = 3.0;
    }

    public class FloodForecastResponseDto
    {
        public int ReportId { get; set; }
        public string RiskLevel { get; set; } = "";
        public string Summary { get; set; } = "";
        public List<string>? Recommendations { get; set; }
        public string? ConfidenceNote { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class FloodForecastAiResult
    {
        public string RiskLevel { get; set; } = "";
        public string Summary { get; set; } = "";
        public List<string>? Recommendations { get; set; }
        public string? ConfidenceNote { get; set; }
        public int? HoursAheadConsidered { get; set; }
    }
}
