namespace Core.DTOs
{
    public class AreaDTO
    {
        public int AreaId { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Address { get; set; }
        public int SensorId { get; set; }
        public string SensorName { get; set; } = string.Empty;
        public long ReadingId { get; set; }
        public float WaterLevelCm { get; set; }
    }
}
