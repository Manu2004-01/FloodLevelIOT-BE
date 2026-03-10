using System;

namespace Core.DTOs
{
    /// <summary>
    /// DTO trả về cho API danh sách / chi tiết bản ghi history (sự kiện ngập).
    /// </summary>
    public class HistoryDTO
    {
        public int HistoryId { get; set; }
        public int LocationId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public float MaxWaterLevel { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
