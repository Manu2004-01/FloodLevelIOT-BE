using System;

namespace Core.DTOs
{
    /// <summary>
    /// DTO trả về cho API danh sách / chi tiết dữ liệu đo cảm biến.
    /// </summary>
    public class SensorReadingDTO
    {
        public long ReadingId { get; set; }
        public int SensorId { get; set; }
        public string Status { get; set; }
        public float WaterLevelCm { get; set; }
        public int BatteryPercent { get; set; }
        public string SignalStrength { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
