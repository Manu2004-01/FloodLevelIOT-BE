using Core.DTOs.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.Sensor
{
    public class ManageSensorDTO
    {
        public int SensorId { get; set; }
        public string SensorName { get; set; }
        public string LocationName { get; set; }
        public string AreaName { get; set; }
        public DateTime InstalledAt { get; set; }
        public string? Status { get; set; }
        public double? WaterLevel { get; set; }
        public string? SignalStrength { get; set; }
    }

    public class SensorDTO
    {
        //Thong so ky thuat
        public int SensorId { get; set; }
        public string SensorCode { get; set; }
        public string Protocol { get; set; }
        public DateTime WarrantyDate { get; set; }
        public string SensorType { get; set; }
        public double WarningThreshold { get; set; }
        public double DangerThreshold { get; set; }
        public int? MaxLevel { get; set; }
        public int? Battery { get; set; }
        // Lich su & Vi tri
        public DateTime InstalledAt { get; set; } // ngay lap dat
        public DateTime CommissionedAt { get; set; } // ngay van hanh
        public string InstalledByStaff { get; set; }
        public LocationDetailDTO Location { get; set; }
        //Bao tri & trang thai
        public double? WaterLevel { get; set; }
        public string? Status { get; set; }
        public DateTime? RecordAt { get; set; }
    }

    public class CreateSensorDTO
    {
        public int LocationId { get; set; }
        public int InstalledBy { get; set; }
        public string Specification { get; set; }
        public string SensorCode { get; set; }
        public string SensorName { get; set; }
        public string Protocol { get; set; }
        public string SensorType { get; set; }
        public double MinThreshold { get; set; }
        public double MaxThreshold { get; set; }
        public int MaxLevel { get; set; } 
    }

    public class UpdateSensorDTO
    {
        public int? LocationId { get; set; }
        public int? InstalledBy { get; set; }
        public string? Specification { get; set; }
        public string? SensorCode { get; set; }
        public string? SensorName { get; set; }
        public string? Protocol { get; set; }
        public string? SensorType { get; set; }
        public double? MinThreshold { get; set; }
        public double? MaxThreshold { get; set; }
        public int? MaxLevel { get; set; }
    }
}