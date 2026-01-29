using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.Sensor
{
    public class SensorDTO
    {
        public int SensorId { get; set; }
        public string SensorCode { get; set; }
        public string SensorName { get; set; }
        public string LocationName { get; set; }
        public string AreaName { get; set; }
        public string SensorStatus { get; set; }
        public DateTime InstalledAt { get; set; }
        
    }

    public class SensorDetailDTO
    {
        public int SensorId { get; set; }
        public string SensorCode { get; set; }
        public string SensorName { get; set; }
        public string SensorType { get; set; }
        public string SensorStatus { get; set; }
        public DateTime? InstalledAt { get; set; }
        public int LocationId { get; set; }

        // Threshold
        public double? MinThreshold { get; set; }
        public double? MaxThreshold { get; set; }
        public string ThresholdType { get; set; }
        public LocationDetailDTO Location { get; set; }
        // Latest reading shown in UI
        public double? LatestWaterLevel { get; set; } // cm
        public string LatestWaterLevelStatus { get; set; }
        public string LatestWaterLevelDisplay
        {
            get => LatestWaterLevel.HasValue ? $"{LatestWaterLevel.Value} cm" : null;
        }
        public bool IsOnline => string.Equals(SensorStatus, "Online", StringComparison.OrdinalIgnoreCase);
    }

    public class LocationDetailDTO
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int AreaId { get; set; }
        public string AreaName { get; set; }
    }

    public class CreateSensorDTO
    {
        public string SensorCode { get; set; }
        public string SensorName { get; set; }
        public string SensorType { get; set; }
        public string SensorStatus { get; set; }
        public DateTime InstalledAt { get; set; }
        public int LocationId { get; set; }
        // Optional threshold fields for creation
        public double? MinThreshold { get; set; }
        public double? MaxThreshold { get; set; }
        public string ThresholdType { get; set; }
    }

    public class UpdateSensorDTO
    {
        public string? SensorName { get; set; }
        public string? SensorType { get; set; }
        public string? SensorStatus { get; set; }
        public int? LocationId { get; set; }
    }

    // DTO cho cập nhật vị trí
    public class UpdateLocationDTO
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
    }

    // DTO cho cập nhật ngưỡng
    public class UpdateThresholdDTO
    {
        public double? MinThreshold { get; set; }
        public double? MaxThreshold { get; set; }
        public string ThresholdType { get; set; }
    }
}