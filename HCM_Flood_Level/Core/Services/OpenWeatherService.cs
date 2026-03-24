using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Core.Services
{
    public class OpenWeatherService : IOpenWeatherService
    {
        private const string ClientName = "OpenWeather";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public OpenWeatherService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<CurrentWeatherDTO?> GetCurrentByCoordinatesAsync(double lat, double lon, CancellationToken cancellationToken = default)
        {
            if (lat is < -90 or > 90 || lon is < -180 or > 180)
                return null;

            var apiKey = _configuration["OpenWeather:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                return null;

            var latStr = lat.ToString(CultureInfo.InvariantCulture);
            var lonStr = lon.ToString(CultureInfo.InvariantCulture);
            var keyEncoded = HttpUtility.UrlEncode(apiKey);
            var url =
                $"https://api.openweathermap.org/data/2.5/weather?lat={latStr}&lon={lonStr}&units=metric&lang=vi&appid={keyEncoded}";

            var client = _httpClientFactory.CreateClient(ClientName);
            client.Timeout = TimeSpan.FromSeconds(15);

            using var response = await client.GetAsync(url, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("cod", out var codEl))
            {
                if (codEl.ValueKind == JsonValueKind.Number && codEl.GetInt32() != 200)
                    return null;
                if (codEl.ValueKind == JsonValueKind.String && codEl.GetString() != "200")
                    return null;
            }

            var dto = new CurrentWeatherDTO();

            if (root.TryGetProperty("coord", out var coord))
            {
                dto.Lat = coord.TryGetProperty("lat", out var la) ? la.GetDouble() : lat;
                dto.Lon = coord.TryGetProperty("lon", out var lo) ? lo.GetDouble() : lon;
            }
            else
            {
                dto.Lat = lat;
                dto.Lon = lon;
            }

            dto.LocationName = root.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String
                ? name.GetString()
                : null;

            if (root.TryGetProperty("main", out var main))
            {
                dto.TemperatureC = main.TryGetProperty("temp", out var t) ? t.GetDouble() : 0;
                dto.FeelsLikeC = main.TryGetProperty("feels_like", out var fl) ? fl.GetDouble() : dto.TemperatureC;
                dto.HumidityPercent = main.TryGetProperty("humidity", out var h) ? h.GetInt32() : 0;
                dto.PressureHpa = main.TryGetProperty("pressure", out var p) ? p.GetInt32() : 0;
            }

            if (root.TryGetProperty("wind", out var wind))
            {
                dto.WindSpeedMps = wind.TryGetProperty("speed", out var s) ? s.GetDouble() : 0;
                dto.WindDeg = wind.TryGetProperty("deg", out var d) ? d.GetInt32() : 0;
                if (wind.TryGetProperty("gust", out var g))
                    dto.WindGustMps = g.GetDouble();
            }

            if (root.TryGetProperty("clouds", out var clouds) && clouds.TryGetProperty("all", out var all))
                dto.CloudsPercent = all.GetInt32();

            if (root.TryGetProperty("visibility", out var vis) && vis.ValueKind == JsonValueKind.Number)
                dto.VisibilityMeters = vis.GetInt32();

            if (root.TryGetProperty("weather", out var weatherArr) && weatherArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var w in weatherArr.EnumerateArray())
                {
                    dto.WeatherMain = w.TryGetProperty("main", out var wm) ? wm.GetString() : null;
                    dto.WeatherDescription = w.TryGetProperty("description", out var wd) ? wd.GetString() : null;
                    dto.WeatherIcon = w.TryGetProperty("icon", out var wi) ? wi.GetString() : null;
                    break;
                }
            }

            if (root.TryGetProperty("rain", out var rain) && rain.TryGetProperty("1h", out var r1h))
                dto.RainMmPerHour = r1h.GetDouble();

            if (root.TryGetProperty("snow", out var snow) && snow.TryGetProperty("1h", out var s1h))
                dto.SnowMmPerHour = s1h.GetDouble();

            if (root.TryGetProperty("dt", out var dt))
                dto.DataCalculatedUnixUtc = dt.GetInt64();

            if (root.TryGetProperty("timezone", out var tz))
                dto.TimezoneOffsetSeconds = tz.GetInt32();

            return dto;
        }
    }
}
