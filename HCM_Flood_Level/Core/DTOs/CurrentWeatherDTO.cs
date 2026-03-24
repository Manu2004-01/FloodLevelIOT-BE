namespace Core.DTOs
{
    public class CurrentWeatherDTO
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string? LocationName { get; set; }
        public double TemperatureC { get; set; }
        public double FeelsLikeC { get; set; }
        public int HumidityPercent { get; set; }
        public int PressureHpa { get; set; }
        public double WindSpeedMps { get; set; }
        public int WindDeg { get; set; }
        public double? WindGustMps { get; set; }
        public int CloudsPercent { get; set; }
        public int? VisibilityMeters { get; set; }
        public string? WeatherMain { get; set; }
        public string? WeatherDescription { get; set; }
        public string? WeatherIcon { get; set; }
        public double? RainMmPerHour { get; set; }
        public double? SnowMmPerHour { get; set; }
        public long DataCalculatedUnixUtc { get; set; }
        public int TimezoneOffsetSeconds { get; set; }
    }
}
