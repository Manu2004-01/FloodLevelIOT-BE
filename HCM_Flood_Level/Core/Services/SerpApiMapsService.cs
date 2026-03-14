using System;
using System.Collections.Generic;
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
    public class SerpApiMapsService : IMapsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public SerpApiMapsService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<object> SearchAsync(MapsSearchDTO dto, CancellationToken ct = default)
        {
            try
            {
                var key = _config["SerpApi:Key"] ?? Environment.GetEnvironmentVariable("SERPAPI_API_KEY");
                if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Thiếu SerpApi API key");

                var q = HttpUtility.UrlEncode(dto.Query);
                var ll = $"@{dto.Lat},{dto.Lng},{dto.Zoom}z";
                var hl = string.IsNullOrWhiteSpace(dto.Hl) ? "vi" : dto.Hl;

                var url = $"https://serpapi.com/search.json?engine=google_maps&type=search&q={q}&ll={ll}&hl={hl}&key={key}";

                var client = _httpClientFactory.CreateClient("SerpApiClient");
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                using var resp = await client.GetAsync(url, ct);
                resp.EnsureSuccessStatusCode();

                var json = await resp.Content.ReadAsStringAsync(ct);
                var doc = JsonDocument.Parse(json);

                var list = new List<object>();
                if (doc.RootElement.TryGetProperty("local_results", out var results) && results.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in results.EnumerateArray())
                    {
                        var obj = new
                        {
                            title = item.GetPropertyOrDefault("title"),
                            place_id = item.GetPropertyOrDefault("place_id"),
                            rating = item.GetPropertyOrDefaultDouble("rating"),
                            reviews = item.GetPropertyOrDefaultInt("reviews"),
                            address = item.GetPropertyOrDefault("address"),
                            types = item.GetPropertyOrDefaultArray("types"),
                            lat = item.TryGetProperty("gps_coordinates", out var gps) ? gps.GetPropertyOrDefaultDouble("latitude") : (double?)null,
                            lng = item.TryGetProperty("gps_coordinates", out var gps2) ? gps2.GetPropertyOrDefaultDouble("longitude") : (double?)null,
                            phone = item.GetPropertyOrDefault("phone"),
                            website = item.GetPropertyOrDefault("website"),
                            hours = item.GetPropertyOrDefault("hours"),
                            open_state = item.GetPropertyOrDefault("open_state"),
                        };
                        list.Add(obj);
                    }
                }

                return new
                {
                    query = dto.Query,
                    center = new { lat = dto.Lat, lng = dto.Lng, zoom = dto.Zoom },
                    count = list.Count,
                    items = list
                };
            }
            catch (Exception)
            {
                // Return empty results on failure instead of crashing
                return new { query = dto.Query, count = 0, items = new List<object>() };
            }
        }

        public async Task<object> GetPlaceDetailsAsync(string placeId, CancellationToken ct = default)
        {
            try
            {
                var key = _config["SerpApi:Key"] ?? Environment.GetEnvironmentVariable("SERPAPI_API_KEY");
                if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Thiếu SerpApi API key");

                var url = $"https://serpapi.com/search.json?engine=google_maps&type=place&place_id={placeId}&key={key}&hl=vi";

                var client = _httpClientFactory.CreateClient("SerpApiClient");
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                using var resp = await client.GetAsync(url, ct);
                resp.EnsureSuccessStatusCode();

                var json = await resp.Content.ReadAsStringAsync(ct);
                var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("place_results", out var item))
                {
                    return new
                    {
                        title = item.GetPropertyOrDefault("title"),
                        place_id = item.GetPropertyOrDefault("place_id"),
                        address = item.GetPropertyOrDefault("address"),
                        lat = item.TryGetProperty("gps_coordinates", out var gps) ? gps.GetPropertyOrDefaultDouble("latitude") : (double?)null,
                        lng = item.TryGetProperty("gps_coordinates", out var gps2) ? gps2.GetPropertyOrDefaultDouble("longitude") : (double?)null
                    };
                }
            }
            catch (Exception)
            {
                // Log exception if needed, or just return null to let the caller handle it
            }

            return null;
        }
    }

    internal static class JsonExtensions
    {
        public static string GetPropertyOrDefault(this JsonElement el, string name)
            => el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        public static double? GetPropertyOrDefaultDouble(this JsonElement el, string name)
            => el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : (double?)null;

        public static int? GetPropertyOrDefaultInt(this JsonElement el, string name)
            => el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : (int?)null;

        public static string[] GetPropertyOrDefaultArray(this JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Array)
            {
                var arr = new List<string>();
                foreach (var x in v.EnumerateArray())
                {
                    if (x.ValueKind == JsonValueKind.String) arr.Add(x.GetString());
                }
                return arr.ToArray();
            }
            return Array.Empty<string>();
        }
    }
}
