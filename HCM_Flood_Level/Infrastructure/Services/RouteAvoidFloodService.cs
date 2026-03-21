using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class RouteAvoidFloodService : IRouteAvoidFloodService
    {
        private readonly ManageDBContext _manageContext;
        private readonly EventsDBContext _eventsContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public RouteAvoidFloodService(
            ManageDBContext manageContext,
            EventsDBContext eventsContext,
            IHttpClientFactory httpClientFactory,
            Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _manageContext = manageContext;
            _eventsContext = eventsContext;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<RouteAvoidFloodResponseDTO> GetAvoidFloodRouteAsync(
            RouteAvoidFloodRequestDTO request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var serpKey = _config["SerpApi:Key"] ?? Environment.GetEnvironmentVariable("SERPAPI_API_KEY");
            if (string.IsNullOrWhiteSpace(serpKey))
                throw new InvalidOperationException("Thiếu SerpApi API key");

            var travelModeId = ParseTravelModeToId(request.TravelMode);
            var floodRadius = request.FloodRadiusMeters <= 0 ? 300 : request.FloodRadiusMeters;

            // 1) Lấy sensor đang ngập theo dữ liệu latest reading
            var floodedSensors = await GetFloodedSensorsAsync(cancellationToken);

            // 2) Lấy route từ SerpApi (nhiều alternatives nếu có)
            var routeAlternatives = await GetRouteAlternativesAsync(
                request,
                serpKey,
                travelModeId,
                cancellationToken);

            if (routeAlternatives.Count == 0)
                return new RouteAvoidFloodResponseDTO();

            // 3) Tính rủi ro theo khoảng cách từ route tới sensor đang ngập
            var scoredAlternatives = new List<RouteAlternativeDTO>(routeAlternatives.Count);
            foreach (var alt in routeAlternatives)
            {
                var scoreResult = ScoreAlternativeAgainstFloods(
                    alt.OverviewPolylinePoints,
                    floodedSensors,
                    floodRadius);

                scoredAlternatives.Add(new RouteAlternativeDTO
                {
                    OverviewPolylinePoints = alt.OverviewPolylinePoints,
                    DistanceMeters = alt.DistanceMeters,
                    DurationSeconds = alt.DurationSeconds,
                    RiskScore = scoreResult.RiskScore,
                    IsFlooded = scoreResult.Warnings.Count > 0
                });

                alt.Warnings = scoreResult.Warnings;
            }

            var recommended = scoredAlternatives
                .OrderBy(a => a.RiskScore)
                .ThenBy(a => a.DurationSeconds ?? int.MaxValue)
                .First();

            // Lấy warnings tương ứng alternative recommended
            var recommendedWarnings = routeAlternatives
                .FirstOrDefault(r => r.OverviewPolylinePoints == recommended.OverviewPolylinePoints)?
                .Warnings ?? new List<RouteFloodWarningDTO>();

            return new RouteAvoidFloodResponseDTO
            {
                RecommendedRoute = recommended,
                IsRecommendedRouteFlooded = recommended.IsFlooded,
                RecommendedWarnings = recommendedWarnings,
                Alternatives = scoredAlternatives
            };
        }

        private async Task<List<FloodSensor>> GetFloodedSensorsAsync(CancellationToken cancellationToken)
        {
            // Lấy sensor + vị trí
            var sensors = await (from s in _manageContext.Sensors.AsNoTracking()
                                 join l in _manageContext.Locations.AsNoTracking()
                                     on s.PlaceId equals l.PlaceId
                                 select new
                                 {
                                     s.SensorId,
                                     s.SensorName,
                                     s.WarningThreshold,
                                     s.DangerThreshold,
                                     l.Latitude,
                                     l.Longitude
                                 }).ToListAsync(cancellationToken);

            if (sensors.Count == 0)
                return new List<FloodSensor>();

            var sensorIds = sensors.Select(x => x.SensorId).Distinct().ToList();
            var latestReadings = await _eventsContext.SensorReadings
                .AsNoTracking()
                .Where(r => sensorIds.Contains(r.SensorId))
                .GroupBy(r => r.SensorId)
                .Select(g => g.OrderByDescending(r => r.RecordedAt).FirstOrDefault())
                .ToListAsync(cancellationToken);

            var readingBySensorId = latestReadings
                .Where(r => r != null)
                .ToDictionary(r => r!.SensorId, r => r!);

            var flooded = new List<FloodSensor>();
            foreach (var s in sensors)
            {
                if (!readingBySensorId.TryGetValue(s.SensorId, out var rd))
                    continue;

                if (string.IsNullOrWhiteSpace(rd.Status))
                    continue;

                // Offline thì coi như không có dữ liệu ngập để cảnh báo
                if (rd.Status.Equals("Offline", StringComparison.OrdinalIgnoreCase))
                    continue;

                float water = rd.WaterLevelCm;

                // Danger ưu tiên hơn Warning
                if (s.DangerThreshold.HasValue && water >= s.DangerThreshold.Value)
                {
                    flooded.Add(new FloodSensor
                    {
                        SensorId = s.SensorId,
                        SensorName = s.SensorName ?? string.Empty,
                        Severity = "Danger",
                        WaterLevelCm = water,
                        WarningThresholdCm = s.WarningThreshold,
                        DangerThresholdCm = s.DangerThreshold,
                        ReadingStatus = rd.Status,
                        Latitude = (double)s.Latitude,
                        Longitude = (double)s.Longitude
                    });
                    continue;
                }

                if (s.WarningThreshold.HasValue && water >= s.WarningThreshold.Value)
                {
                    flooded.Add(new FloodSensor
                    {
                        SensorId = s.SensorId,
                        SensorName = s.SensorName ?? string.Empty,
                        Severity = "Warning",
                        WaterLevelCm = water,
                        WarningThresholdCm = s.WarningThreshold,
                        DangerThresholdCm = s.DangerThreshold,
                        ReadingStatus = rd.Status,
                        Latitude = (double)s.Latitude,
                        Longitude = (double)s.Longitude
                    });
                }
            }

            return flooded;
        }

        private async Task<List<RouteAlternativeInternal>> GetRouteAlternativesAsync(
            RouteAvoidFloodRequestDTO request,
            string serpKey,
            int travelModeId,
            CancellationToken cancellationToken)
        {
            // Build URL for SerpApi Directions.
            // Note: SerpApi đôi khi có khác biệt giữa /search và /search.json theo account/phiên bản.
            // Để giảm lỗi môi trường/network (remote đóng kết nối), ta thử cả 2.
            var hl = "vi";

            string startPart;
            if (request.StartLat.HasValue && request.StartLng.HasValue)
            {
                // SerpApi directions docs thường dùng dạng `lat,lng` (dấu phẩy không cần encode).
                // Một số trường hợp encode `%2C` lại khiến SerpApi xử lý sai ở tầng parsing.
                startPart = $"start_coords={Fmt(request.StartLat.Value)},{Fmt(request.StartLng.Value)}";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.StartAddress))
                    throw new InvalidOperationException("Thiếu StartAddress hoặc StartLat/StartLng");
                startPart = $"start_addr={Uri.EscapeDataString(request.StartAddress)}";
            }

            string endPart;
            if (request.EndLat.HasValue && request.EndLng.HasValue)
            {
                endPart = $"end_coords={Fmt(request.EndLat.Value)},{Fmt(request.EndLng.Value)}";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.EndAddress))
                    throw new InvalidOperationException("Thiếu EndAddress hoặc EndLat/EndLng");
                endPart = $"end_addr={Uri.EscapeDataString(request.EndAddress)}";
            }

            var client = _httpClientFactory.CreateClient("SerpApiClient");
            client.Timeout = TimeSpan.FromSeconds(30);

            // Giống cách SerpApiMapsService set header để tránh bị WAF chặn/đóng kết nối.
            if (!client.DefaultRequestHeaders.UserAgent.Any())
            {
                client.DefaultRequestHeaders.Add(
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            }

            // Chỉ định accept json để server trả đúng format.
            if (!client.DefaultRequestHeaders.Accept.Any())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }

            string json = string.Empty;
            Exception? lastError = null;

            var baseUrls = new[]
            {
                // Theo docs hiện tại
                "https://serpapi.com/search?engine=google_maps_directions",
                // Một số trường hợp vẫn dùng được /search.json
                "https://serpapi.com/search.json?engine=google_maps_directions"
            };

            string? lastUrl = null;

            var maxAttempts = 5;
            foreach (var baseUrl in baseUrls)
            {
                var url =
                    baseUrl +
                    $"&key={Uri.EscapeDataString(serpKey)}" +
                    $"&hl={Uri.EscapeDataString(hl)}" +
                    $"&travel_mode={travelModeId}" +
                    $"&{startPart}" +
                    $"&{endPart}";

                for (var attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        lastUrl = url;
                        using var resp = await client.GetAsync(url, cancellationToken);
                        resp.EnsureSuccessStatusCode();
                        json = await resp.Content.ReadAsStringAsync(cancellationToken);
                        lastError = null;
                        break;
                    }
                    catch (Exception ex) when (attempt < 3)
                    {
                        lastError = ex;
                        var delayMs = (int)(500 * Math.Pow(attempt, 1.8)) + Random.Shared.Next(0, 250);
                        await Task.Delay(delayMs, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        lastError = ex;
                        break;
                    }
                }

                if (lastError == null && !string.IsNullOrWhiteSpace(json))
                    break;
            }

            if (lastError != null)
            {
                // Tránh trả 500 cho client khi SerpApi chập chờn đóng kết nối.
                // GetAvoidFloodRouteAsync sẽ trả response mặc định (recommendedRoute=null).
                return new List<RouteAlternativeInternal>();
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var results = new List<RouteAlternativeInternal>();

            // SerpApi có thể trả "routes" dạng array hoặc object tùy trường hợp/account.
            if (root.TryGetProperty("routes", out var routesEl))
            {
                if (routesEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var routeEl in routesEl.EnumerateArray())
                    {
                        var alt = TryParseRouteAlternative(routeEl);
                        if (alt != null) results.Add(alt);
                    }
                }
                else if (routesEl.ValueKind == JsonValueKind.Object)
                {
                    var alt = TryParseRouteAlternative(routesEl);
                    if (alt != null) results.Add(alt);
                }
            }
            else if (root.TryGetProperty("route", out var routeEl))
            {
                // Fallback: đôi khi SerpApi trả route đơn dưới key "route"
                if (routeEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in routeEl.EnumerateArray())
                    {
                        var alt = TryParseRouteAlternative(r);
                        if (alt != null) results.Add(alt);
                    }
                }
                else if (routeEl.ValueKind == JsonValueKind.Object)
                {
                    var alt = TryParseRouteAlternative(routeEl);
                    if (alt != null) results.Add(alt);
                }
            }

            // Nếu vẫn chưa parse được gì (SerpApi trả JSON schema khác), duyệt toàn bộ cây JSON
            // để nhặt mọi node có `overview_polyline.points`.
            if (results.Count == 0)
            {
                var seenPoly = new HashSet<string>(StringComparer.Ordinal);
                CollectRouteAlternativesRecursive(root, results, seenPoly);
            }

            // SerpApi engine=google_maps_directions trả mảng `directions` + gps trong trips/details,
            // không dùng schema Google Directions `routes[].overview_polyline`.
            if (results.Count == 0)
            {
                foreach (var alt in ParseSerpApiDirectionsAlternatives(root, travelModeId))
                    results.Add(alt);
            }

            return results;
        }

        /// <summary>
        /// Parse định dạng JSON thực tế của SerpApi Google Maps Directions (mảng directions).
        /// Ghép các điểm gps_coordinates theo thứ tự để encode polyline cho map + tính khoảng cách tới sensor.
        /// </summary>
        private static List<RouteAlternativeInternal> ParseSerpApiDirectionsAlternatives(
            JsonElement root,
            int travelModeId)
        {
            var results = new List<RouteAlternativeInternal>();
            if (!root.TryGetProperty("directions", out var directionsEl) ||
                directionsEl.ValueKind != JsonValueKind.Array)
                return results;

            var filterLabel = TravelModeIdToSerpTravelModeLabel(travelModeId);

            foreach (var dir in directionsEl.EnumerateArray())
            {
                if (filterLabel != null)
                {
                    if (!dir.TryGetProperty("travel_mode", out var tmEl) ||
                        tmEl.ValueKind != JsonValueKind.String ||
                        !string.Equals(tmEl.GetString(), filterLabel, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                var points = CollectGpsPointsFromSerpDirection(dir, root);
                if (points.Count < 2)
                    continue;

                var encoded = EncodePolyline(points);
                if (string.IsNullOrEmpty(encoded))
                    continue;

                int? distanceMeters = null;
                int? durationSeconds = null;
                if (dir.TryGetProperty("distance", out var distEl) && distEl.ValueKind == JsonValueKind.Number)
                    distanceMeters = distEl.TryGetInt32(out var d) ? d : (int)distEl.GetDouble();
                if (dir.TryGetProperty("duration", out var durEl) && durEl.ValueKind == JsonValueKind.Number)
                    durationSeconds = durEl.TryGetInt32(out var t) ? t : (int)durEl.GetDouble();

                results.Add(new RouteAlternativeInternal
                {
                    OverviewPolylinePoints = encoded,
                    DistanceMeters = distanceMeters,
                    DurationSeconds = durationSeconds,
                    Warnings = new List<RouteFloodWarningDTO>()
                });
            }

            return results;
        }

        /// <summary>
        /// SerpApi dùng chuỗi travel_mode ("Driving", "Walking", ...). ID giống tham số API.
        /// </summary>
        private static string? TravelModeIdToSerpTravelModeLabel(int travelModeId) =>
            travelModeId switch
            {
                0 => "Driving",
                1 => "Cycling",
                2 => "Walking",
                3 => "Transit",
                4 => "Flight",
                6 => "Driving", // Best: dùng ô tô cho tránh ngập
                9 => null,      // Two-wheeler — tên chuỗi có thể khác theo locale; lấy hướng đầu tiên khớp GPS
                _ => "Driving"
            };

        private static List<(double lat, double lng)> CollectGpsPointsFromSerpDirection(
            JsonElement directionEl,
            JsonElement root)
        {
            var points = new List<(double lat, double lng)>();

            if (directionEl.TryGetProperty("trips", out var trips) && trips.ValueKind == JsonValueKind.Array)
            {
                foreach (var trip in trips.EnumerateArray())
                {
                    if (trip.TryGetProperty("details", out var details) && details.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var detail in details.EnumerateArray())
                        {
                            if (TryGetGpsCoordinates(detail, out var lat, out var lng))
                                AppendPointIfDistinct(points, lat, lng);
                        }
                    }
                }
            }

            if (points.Count < 2 && root.TryGetProperty("places_info", out var places) &&
                places.ValueKind == JsonValueKind.Array && places.GetArrayLength() >= 2)
            {
                points.Clear();
                if (TryGetGpsFromPlace(places[0], out var aLat, out var aLng) &&
                    TryGetGpsFromPlace(places[places.GetArrayLength() - 1], out var bLat, out var bLng))
                {
                    points.Add((aLat, aLng));
                    points.Add((bLat, bLng));
                }
            }

            return points;
        }

        private static void AppendPointIfDistinct(List<(double lat, double lng)> points, double lat, double lng)
        {
            if (points.Count > 0)
            {
                var last = points[^1];
                if (HaversineMeters(last.lat, last.lng, lat, lng) < 1.0)
                    return;
            }
            points.Add((lat, lng));
        }

        private static bool TryGetGpsCoordinates(JsonElement el, out double lat, out double lng)
        {
            lat = 0;
            lng = 0;
            if (!el.TryGetProperty("gps_coordinates", out var gps) || gps.ValueKind != JsonValueKind.Object)
                return false;
            return TryReadLatLng(gps, out lat, out lng);
        }

        private static bool TryGetGpsFromPlace(JsonElement placeEl, out double lat, out double lng)
        {
            lat = 0;
            lng = 0;
            if (!placeEl.TryGetProperty("gps_coordinates", out var gps) || gps.ValueKind != JsonValueKind.Object)
                return false;
            return TryReadLatLng(gps, out lat, out lng);
        }

        private static bool TryReadLatLng(JsonElement gps, out double lat, out double lng)
        {
            lat = 0;
            lng = 0;
            if (!gps.TryGetProperty("latitude", out var latEl) || latEl.ValueKind != JsonValueKind.Number)
                return false;
            if (!gps.TryGetProperty("longitude", out var lngEl) || lngEl.ValueKind != JsonValueKind.Number)
                return false;
            lat = latEl.GetDouble();
            lng = lngEl.GetDouble();
            return true;
        }

        private static string EncodePolyline(IReadOnlyList<(double lat, double lng)> points)
        {
            if (points == null || points.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            var lastLat = 0;
            var lastLng = 0;
            foreach (var (lat, lng) in points)
            {
                var iLat = (int)Math.Round(lat * 1e5, MidpointRounding.AwayFromZero);
                var iLng = (int)Math.Round(lng * 1e5, MidpointRounding.AwayFromZero);
                EncodeSignedNumber(iLat - lastLat, sb);
                EncodeSignedNumber(iLng - lastLng, sb);
                lastLat = iLat;
                lastLng = iLng;
            }

            return sb.ToString();
        }

        private static void EncodeSignedNumber(int num, StringBuilder result)
        {
            var sgnNum = (uint)(num < 0 ? ~(num << 1) : (num << 1));
            while (sgnNum >= 0x20)
            {
                result.Append((char)((0x20 | (sgnNum & 0x1f)) + 63));
                sgnNum >>= 5;
            }
            result.Append((char)(sgnNum + 63));
        }

        private static RouteAlternativeInternal? TryParseRouteAlternative(JsonElement routeEl)
        {
            if (!routeEl.TryGetProperty("overview_polyline", out var overviewEl))
                return null;

            if (!overviewEl.TryGetProperty("points", out var pointsEl))
                return null;

            var points = pointsEl.GetString();
            if (string.IsNullOrWhiteSpace(points))
                return null;

            int? distanceMeters = null;
            int? durationSeconds = null;

            if (routeEl.TryGetProperty("legs", out var legsEl) &&
                legsEl.ValueKind == JsonValueKind.Array &&
                legsEl.GetArrayLength() > 0)
            {
                var leg0 = legsEl[0];

                if (leg0.TryGetProperty("distance", out var distanceEl) &&
                    distanceEl.TryGetProperty("value", out var distanceValueEl) &&
                    distanceValueEl.ValueKind == JsonValueKind.Number)
                {
                    distanceMeters = distanceValueEl.GetInt32();
                }

                if (leg0.TryGetProperty("duration", out var durationEl) &&
                    durationEl.TryGetProperty("value", out var durationValueEl) &&
                    durationValueEl.ValueKind == JsonValueKind.Number)
                {
                    durationSeconds = durationValueEl.GetInt32();
                }
            }

            return new RouteAlternativeInternal
            {
                OverviewPolylinePoints = points,
                DistanceMeters = distanceMeters,
                DurationSeconds = durationSeconds,
                Warnings = new List<RouteFloodWarningDTO>()
            };
        }

        private static void CollectRouteAlternativesRecursive(
            JsonElement element,
            List<RouteAlternativeInternal> results,
            HashSet<string> seenPoly)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var alt = TryParseRouteAlternative(element);
                if (alt != null && seenPoly.Add(alt.OverviewPolylinePoints))
                    results.Add(alt);

                foreach (var prop in element.EnumerateObject())
                {
                    CollectRouteAlternativesRecursive(prop.Value, results, seenPoly);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    CollectRouteAlternativesRecursive(item, results, seenPoly);
                }
            }
        }

        private (double RiskScore, List<RouteFloodWarningDTO> Warnings) ScoreAlternativeAgainstFloods(
            string overviewPolylinePoints,
            List<FloodSensor> floodedSensors,
            double radiusMeters)
        {
            if (string.IsNullOrWhiteSpace(overviewPolylinePoints))
                return (0, new List<RouteFloodWarningDTO>());

            var routePoints = DecodePolyline(overviewPolylinePoints);
            if (routePoints.Count == 0)
                return (0, new List<RouteFloodWarningDTO>());

            var sampledRoutePoints = Sample(routePoints, 200);

            double score = 0;
            var warnings = new List<RouteFloodWarningDTO>();

            foreach (var sensor in floodedSensors)
            {
                double minDist = double.MaxValue;
                foreach (var p in sampledRoutePoints)
                {
                    var d = HaversineMeters(p.lat, p.lng, sensor.Latitude, sensor.Longitude);
                    if (d < minDist) minDist = d;
                    if (minDist <= 10) break; // khá gần rồi
                }

                if (minDist <= radiusMeters)
                {
                    var weight = sensor.Severity == "Danger" ? 3.0 : 2.0;
                    score += weight * (1.0 - (minDist / radiusMeters));

                    warnings.Add(new RouteFloodWarningDTO
                    {
                        SensorId = sensor.SensorId,
                        SensorName = sensor.SensorName,
                        Severity = sensor.Severity,
                        MinDistanceMeters = minDist,
                        SensorLatitude = sensor.Latitude,
                        SensorLongitude = sensor.Longitude,
                        WaterLevelCm = sensor.WaterLevelCm,
                        WarningThresholdCm = sensor.WarningThresholdCm,
                        DangerThresholdCm = sensor.DangerThresholdCm,
                        ReadingStatus = sensor.ReadingStatus
                    });
                }
            }

            warnings = warnings
                .OrderBy(w => w.MinDistanceMeters)
                .ThenByDescending(w => w.Severity == "Danger")
                .ToList();

            return (score, warnings);
        }

        private static List<(double lat, double lng)> DecodePolyline(string encoded)
        {
            // Google encoded polyline algorithm.
            // https://developers.google.com/maps/documentation/utilities/polylinealgorithm
            var poly = new List<(double lat, double lng)>();
            if (string.IsNullOrEmpty(encoded))
                return poly;

            int index = 0;
            int lat = 0;
            int lng = 0;

            while (index < encoded.Length)
            {
                lat += DecodeNextValue(encoded, ref index);
                lng += DecodeNextValue(encoded, ref index);

                poly.Add((lat / 1e5, lng / 1e5));
            }

            return poly;
        }

        private static int DecodeNextValue(string encoded, ref int index)
        {
            int result = 0;
            int shift = 0;
            int b;
            do
            {
                b = encoded[index++] - 63;
                result |= (b & 0x1f) << shift;
                shift += 5;
            } while (b >= 0x20 && index < encoded.Length);

            int delta = ((result & 1) == 1) ? ~(result >> 1) : (result >> 1);
            return delta;
        }

        private static List<(double lat, double lng)> Sample(List<(double lat, double lng)> points, int maxPoints)
        {
            if (points.Count <= maxPoints) return points;
            var result = new List<(double lat, double lng)>(maxPoints);
            var step = (double)points.Count / maxPoints;
            for (int i = 0; i < maxPoints; i++)
            {
                int idx = (int)Math.Round(i * step);
                idx = Math.Clamp(idx, 0, points.Count - 1);
                result.Add(points[idx]);
            }
            return result.Distinct().ToList();
        }

        private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // meters
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double DegreesToRadians(double deg) => deg * (Math.PI / 180.0);

        private static int ParseTravelModeToId(string? travelMode)
        {
            if (string.IsNullOrWhiteSpace(travelMode))
                return 0; // driving

            var t = travelMode.Trim().ToLowerInvariant();
            return t switch
            {
                "driving" => 0,
                "walking" => 2,
                "transit" => 3,
                "cycling" => 1,
                "best" => 6,
                // Nếu front-end gửi trực tiếp số (ví dụ "0", "1"...)
                _ => int.TryParse(t, out var v) ? v : 0
            };
        }


        private static string Fmt(double v) => v.ToString("0.######", CultureInfo.InvariantCulture);

        private sealed class FloodSensor
        {
            public int SensorId { get; set; }
            public string SensorName { get; set; } = string.Empty;
            public string Severity { get; set; } = string.Empty;
            public float WaterLevelCm { get; set; }
            public float? WarningThresholdCm { get; set; }
            public float? DangerThresholdCm { get; set; }
            public string ReadingStatus { get; set; } = string.Empty;
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        private sealed class RouteAlternativeInternal
        {
            public string OverviewPolylinePoints { get; set; } = string.Empty;
            public int? DistanceMeters { get; set; }
            public int? DurationSeconds { get; set; }
            public List<RouteFloodWarningDTO> Warnings { get; set; } = new List<RouteFloodWarningDTO>();
        }
    }
}

