using System;

namespace Core.DTOs
{
    /// <summary>
    /// DTO trả về cho API danh sách / chi tiết sự kiện ngập.
    /// </summary>
    public class FloodEventDTO
    {
        public int EventId { get; set; }
        public int SensorId { get; set; }
        public int LocationId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public float MaxWaterLevel { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
