namespace Core.DTOs
{
    public class RouteAvoidFloodRequestDTO
    {
        // Input có thể là address hoặc lat/lng (ưu tiên lat/lng nếu có).
        public string? StartAddress { get; set; }
        public double? StartLat { get; set; }
        public double? StartLng { get; set; }

        public string? EndAddress { get; set; }
        public double? EndLat { get; set; }
        public double? EndLng { get; set; }

        // Mặc định "Driving"
        public string? TravelMode { get; set; } = "Driving";

        // Bán kính quanh sensor để coi đường đi "đi vào vùng ngập"
        public double FloodRadiusMeters { get; set; } = 300;
    }
}

