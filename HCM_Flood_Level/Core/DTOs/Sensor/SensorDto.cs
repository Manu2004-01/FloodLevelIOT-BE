using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs.Sensor
{
    public class SensorDto
    {
        public int SensorId { get; set; }
        public string SensorCode { get; set; }
        public string SensorName { get; set; }
        public string SensorType { get; set; }
        public string SensorStatus { get; set; }
        public DateTime InstalledAt { get; set; }
        public int LocationId { get; set; }
        public LocationDto Location { get; set; }
    }

    public class SensorCreateDto
    {
        public string SensorCode { get; set; }
        public string SensorName { get; set; }
        public string SensorType { get; set; }
        public string SensorStatus { get; set; }
        public DateTime InstalledAt { get; set; }
        public int LocationId { get; set; }
    }

    public class SensorUpdateDto
    {
        public string SensorName { get; set; }
        public string SensorType { get; set; }
        public string SensorStatus { get; set; }
        public int LocationId { get; set; }
    }

    public class SensorLocationUpdateDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
    }

    //public class SensorThresholdUpdateDto
    //{
    //    public double MinThreshold { get; set; }
    //    public double MaxThreshold { get; set; }
    //    public string ThresholdType { get; set; }
    //}

    public class LocationDto
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int AreaId { get; set; }
        public string AreaName { get; set; }
    }
}