using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class FloodForecastService : IFloodForecastService
    {
        private static readonly JsonSerializerOptions JsonReadOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly ManageDBContext _manage;
        private readonly EventsDBContext _events;
        private readonly IOpenWeatherService _openWeather;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public FloodForecastService(
            ManageDBContext manage,
            EventsDBContext events,
            IOpenWeatherService openWeather,
            IUnitOfWork unitOfWork,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _manage = manage;
            _events = events;
            _openWeather = openWeather;
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<FloodForecastResponseDto?> RunForecastForCitizenAsync(
            double latitude,
            double longitude,
            double radiusKm = 3.0,
            CancellationToken cancellationToken = default)
        {
            if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
                return null;

            var effectiveRadiusKm = radiusKm <= 0 ? 3.0 : radiusKm;
            var lat = latitude;
            var lon = longitude;

            var weather = await _openWeather.GetCurrentByCoordinatesAsync(lat, lon, cancellationToken);

            var latDelta = effectiveRadiusKm / 111.0;
            var cosLat = Math.Cos(lat * Math.PI / 180.0);
            var lonDelta = effectiveRadiusKm / (111.0 * Math.Max(Math.Abs(cosLat), 0.01));

            var nearbyLocations = await _manage.Locations
                .AsNoTracking()
                .Where(l =>
                    (double)l.Latitude >= lat - latDelta &&
                    (double)l.Latitude <= lat + latDelta &&
                    (double)l.Longitude >= lon - lonDelta &&
                    (double)l.Longitude <= lon + lonDelta)
                .Select(l => new
                {
                    l.PlaceId,
                    l.Title,
                    l.Address,
                    Latitude = (double)l.Latitude,
                    Longitude = (double)l.Longitude
                })
                .ToListAsync(cancellationToken);

            var locationById = nearbyLocations
                .Select(l => new
                {
                    l.PlaceId,
                    l.Title,
                    l.Address,
                    l.Latitude,
                    l.Longitude,
                    DistanceKm = HaversineKm(lat, lon, l.Latitude, l.Longitude)
                })
                .Where(x => x.DistanceKm <= effectiveRadiusKm)
                .ToDictionary(x => x.PlaceId, x => x);

            if (locationById.Count == 0)
                return null;

            var placeIds = locationById.Keys.ToList();
            var sensors = await _manage.Sensors
                .AsNoTracking()
                .Where(s => placeIds.Contains(s.PlaceId))
                .Select(s => new
                {
                    s.SensorId,
                    s.PlaceId,
                    s.SensorCode,
                    s.SensorName,
                    s.WarningThreshold,
                    s.DangerThreshold
                })
                .ToListAsync(cancellationToken);

            var sensorIds = sensors.Select(s => s.SensorId).Distinct().ToList();
            var readings = sensorIds.Count == 0
                ? new List<SensorReading>()
                : (await _unitOfWork.ManageSensorRepository.GetLatestReadingsForSensorIdsAsync(sensorIds)).ToList();

            var readingsBySensorId = readings.ToDictionary(r => r.SensorId, r => r);
            var activeSensors = sensors
                .Where(s => readingsBySensorId.ContainsKey(s.SensorId))
                .ToList();

            if (activeSensors.Count == 0)
                return null;

            var histories = await _events.Histories
                .AsNoTracking()
                .Where(h => placeIds.Contains(h.LocationId))
                .OrderByDescending(h => h.StartTime)
                .Take(30)
                .ToListAsync(cancellationToken);

            var inputsObject = new
            {
                citizen = new
                {
                    latitude = lat,
                    longitude = lon,
                    radiusKm = effectiveRadiusKm
                },
                weather,
                nearbySensors = activeSensors.Select(s => new
                {
                    s.PlaceId,
                    placeTitle = locationById[s.PlaceId].Title,
                    placeAddress = locationById[s.PlaceId].Address,
                    distanceKm = Math.Round(locationById[s.PlaceId].DistanceKm, 3),
                    s.SensorId,
                    s.SensorCode,
                    s.SensorName,
                    warningThresholdCm = (double?)s.WarningThreshold,
                    dangerThresholdCm = (double?)s.DangerThreshold,
                    latestReading = readingsBySensorId[s.SensorId]
                }),
                recentHistories = histories.Select(h => new
                {
                    h.HistoryId,
                    h.StartTime,
                    h.EndTime,
                    h.MaxWaterLevel,
                    severity = h.Severity.ToString()
                })
            };

            var inputsJson = JsonSerializer.Serialize(inputsObject, new JsonSerializerOptions { WriteIndented = false });
            var prompt = BuildUserPrompt(lat, lon, effectiveRadiusKm, inputsJson);

            var geminiRaw = await CallGeminiAsync(prompt, cancellationToken);
            var normalized = NormalizeModelJson(geminiRaw);
            FloodForecastAiResult? ai;
            try
            {
                ai = JsonSerializer.Deserialize<FloodForecastAiResult>(normalized, JsonReadOptions);
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Gemini tr? v? JSON kh?ng d?c du?c. Th? l?i sau.");
            }

            var risk = NormalizeRiskLevel(ai?.RiskLevel);
            var summary = string.IsNullOrWhiteSpace(ai?.Summary)
                ? "Kh?ng c? t?m t?t t? m? h?nh. Xem tru?ng forecastDataJson."
                : ai!.Summary.Trim();

            var modelName = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
            using var inputsDoc = JsonDocument.Parse(inputsJson);
            using var aiDoc = JsonDocument.Parse(normalized);
            var fullPayload = new Dictionary<string, object?>
            {
                ["generatedAtUtc"] = DateTime.UtcNow,
                ["geminiModel"] = modelName,
                ["inputs"] = inputsDoc.RootElement.Clone(),
                ["ai"] = aiDoc.RootElement.Clone()
            };
            var forecastDataJson = JsonSerializer.Serialize(fullPayload);

            var report = new Report
            {
                Description = summary,
                ForecastRiskLevel = risk,
                ForecastDataJson = forecastDataJson,
                CreatedAt = DateTime.UtcNow
            };

            await _events.Reports.AddAsync(report, cancellationToken);
            try
            {
                await _events.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsMissingReportTable(ex))
            {
                // Auto-heal: create report table on EventsDb once, then retry save.
                await EnsureReportTableExistsAsync(cancellationToken);
                await _events.SaveChangesAsync(cancellationToken);
            }

            return new FloodForecastResponseDto
            {
                ReportId = report.ReportId,
                RiskLevel = risk,
                Summary = summary,
                Recommendations = ai?.Recommendations,
                ConfidenceNote = ai?.ConfidenceNote,
                CreatedAtUtc = report.CreatedAt
            };
        }

        private async Task<string> CallGeminiAsync(string userPrompt, CancellationToken cancellationToken)
        {
            var apiKey = _configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Chua c?u h?nh Gemini API key (Gemini:ApiKey ho?c GEMINI_API_KEY).");

            var model = (_configuration["Gemini:Model"] ?? "gemini-1.5-flash").Trim();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

            var body = new Dictionary<string, object?>
            {
                ["contents"] = new object[]
                {
                    new Dictionary<string, object?>
                    {
                        ["role"] = "user",
                        ["parts"] = new object[]
                        {
                            new Dictionary<string, object?> { ["text"] = userPrompt }
                        }
                    }
                },
                ["generationConfig"] = new Dictionary<string, object?>
                {
                    ["temperature"] = 0.35
                }
            };

            var json = JsonSerializer.Serialize(body);
            var client = _httpClientFactory.CreateClient("Gemini");
            client.Timeout = TimeSpan.FromSeconds(90);

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.TryAddWithoutValidation("x-goog-api-key", apiKey);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var resp = await client.SendAsync(req, cancellationToken);
            var respText = await resp.Content.ReadAsStringAsync(cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                var googleMsg = TryGetGoogleErrorMessage(respText);
                throw new InvalidOperationException(
                    $"Gemini HTTP {(int)resp.StatusCode}: {googleMsg ?? TruncateForMessage(respText, 500)}");
            }

            using var doc = JsonDocument.Parse(respText);
            var root = doc.RootElement;

            if (root.TryGetProperty("promptFeedback", out var fb) &&
                fb.TryGetProperty("blockReason", out var br))
            {
                var reason = br.ValueKind == JsonValueKind.String ? br.GetString() : br.ToString();
                throw new InvalidOperationException($"Gemini ch?n prompt (blockReason: {reason}).");
            }

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                throw new InvalidOperationException($"Gemini kh?ng tr? candidates. Ph?n h?i: {TruncateForMessage(respText, 800)}");

            var first = candidates[0];
            var finishReason = GetFinishReasonString(first);
            if (string.Equals(finishReason, "SAFETY", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Gemini d?ng v? SAFETY (n?i dung b? ch?n).");

            if (!first.TryGetProperty("content", out var contentEl) ||
                !contentEl.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
                throw new InvalidOperationException($"Gemini thi?u content/parts (finishReason={finishReason}).");

            var part0 = parts[0];
            if (!part0.TryGetProperty("text", out var textEl))
                throw new InvalidOperationException("Gemini part kh?ng c? tru?ng text.");

            var text = textEl.GetString();
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Gemini tr? text r?ng.");

            return text;
        }

        private static string? TryGetGoogleErrorMessage(string respText)
        {
            try
            {
                using var doc = JsonDocument.Parse(respText);
                if (doc.RootElement.TryGetProperty("error", out var err) &&
                    err.TryGetProperty("message", out var msg) &&
                    msg.ValueKind == JsonValueKind.String)
                    return msg.GetString();
            }
            catch (JsonException)
            {
            }

            return null;
        }

        private static string? GetFinishReasonString(JsonElement candidate)
        {
            if (!candidate.TryGetProperty("finishReason", out var fr)) return null;
            return fr.ValueKind == JsonValueKind.String ? fr.GetString() : fr.ToString();
        }

        private static string TruncateForMessage(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return "(empty)";
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length <= maxLen ? s : s[..maxLen] + "?";
        }

        private static string BuildUserPrompt(double latitude, double longitude, double radiusKm, string inputsJson)
        {
            return $@"B?n h? tr? c?nh b?o ng?p ?ng ?? th? Vi?t Nam (kh?ng thay th? c?nh b?o ch?nh th?c c?a c? quan nh? n??c).
H? th?ng AL/ML d? d??c hu?n luy?n v?i d? li?u l?ch s? ng?p l?t t?i TP.HCM trong 20 n?m t? 2006 ??n 2026.
D?a tr?n ki?n th?c l?ch s? n?y k?t h?p th?i ti?t OpenWeather + sensor, h?y d? b?o cho citizen trong b?n k?nh {radiusKm:0.##}km.

T?a ?? m?u: ({latitude}, {longitude})

D? li?u ??u v?o (JSON):
{inputsJson}

Tr? v? DUY NH?T m?t JSON h?p l?, kh?ng markdown, v?i c?c kh?a:
- ""riskLevel"": ""Low"", ""Medium"", ""High"".
- ""summary"": 2-4 c?u ti?nh Vi?t n?u r? t?nh tr?ng d?a v?o m? h?nh l?ch s? 20 n?m (2006-2026).
- ""recommendations"": m?ng 2-5 g?i ? h?nh ??ng.
- ""confidenceNote"": ghi ch? ?? tin c?y.
- ""hoursAheadConsidered"": s? nguy?n (v? d? 12).";
        }

        private static string NormalizeModelJson(string raw)
        {
            var t = raw.Trim();
            if (t.StartsWith("```json", StringComparison.OrdinalIgnoreCase)) t = t["```json".Length..].Trim();
            else if (t.StartsWith("```", StringComparison.Ordinal)) t = t[3..].Trim();
            if (t.EndsWith("```", StringComparison.Ordinal)) t = t[..^3].Trim();
            return t;
        }

        private static string NormalizeRiskLevel(string? level)
        {
            if (string.IsNullOrWhiteSpace(level)) return "Medium";
            var x = level.Trim();
            if (string.Equals(x, "Low", StringComparison.OrdinalIgnoreCase)) return "Low";
            if (string.Equals(x, "Medium", StringComparison.OrdinalIgnoreCase)) return "Medium";
            if (string.Equals(x, "High", StringComparison.OrdinalIgnoreCase)) return "High";
            if (x.Contains("th?p", StringComparison.OrdinalIgnoreCase)) return "Low";
            if (x.Contains("cao", StringComparison.OrdinalIgnoreCase)) return "High";
            return "Medium";
        }

        private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Pow(Math.Sin(dLon / 2), 2);
            var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

        private static bool IsMissingReportTable(DbUpdateException ex)
        {
            var all = ex.ToString();
            return all.Contains("relation \"report\" does not exist", StringComparison.OrdinalIgnoreCase)
                || all.Contains("42P01", StringComparison.OrdinalIgnoreCase);
        }

        private async Task EnsureReportTableExistsAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
CREATE TABLE IF NOT EXISTS report (
    report_id SERIAL PRIMARY KEY,
    description TEXT NULL,
    forecast_risk_level TEXT NULL,
    forecast_data_json TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

ALTER TABLE report ADD COLUMN IF NOT EXISTS forecast_risk_level TEXT NULL;
ALTER TABLE report ADD COLUMN IF NOT EXISTS forecast_data_json TEXT NULL;
";

            await _events.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }
    }
}
