using AutoMapper;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Core.Sharing;
using Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ManageSensorRepository : GenericRepository<Sensor>, IManageSensorRepository
    {
        private readonly ManageDBContext _context;
        private readonly EventsDBContext _eventsContext;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;
        private readonly IMapsService _mapsService;

        public ManageSensorRepository(ManageDBContext context, EventsDBContext eventsContext, IFileProvider fileProvider, IMapper mapper, IMapsService mapsService) : base(context)
        {
            _context = context;
            _eventsContext = eventsContext;
            _fileProvider = fileProvider;
            _mapper = mapper;
            _mapsService = mapsService;
        }

        public async Task<IEnumerable<Sensor>> GetAllSensorsAsync(EntityParam param)
        {
            var query = _context.Sensors
                .Include(s => s.Location)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                query = query.Where(s =>
                    s.SensorCode.Contains(param.Search) ||
                    s.SensorName.Contains(param.Search) ||
                    s.SensorType.Contains(param.Search) ||
                    s.Location.Title.Contains(param.Search)
                );
            }

            return await query
                .OrderByDescending(s => s.InstalledAt)
                .Skip((param.Pagenumber - 1) * param.Pagesize)
                .Take(param.Pagesize)
                .ToListAsync();
        }

        public async Task<bool> AddNewSensorAsync(CreateSensorDTO dto)
        {
            // 1. Check if location exists, if not create it
            var location = await _context.Locations.FindAsync(dto.PlaceId);
            if (location == null)
            {
                // If DTO lacks title/lat/lng, fetch from SerpApi
                if (string.IsNullOrEmpty(dto.Title) || dto.Latitude == 0 || dto.Longitude == 0)
                {
                    var details = await _mapsService.GetPlaceDetailsAsync(dto.PlaceId);
                    if (details != null)
                    {
                        // Parse detail fields safely without using dynamic keyword which can cause RuntimeBinderException
                        var json = System.Text.Json.JsonSerializer.Serialize(details);
                        var doc = System.Text.Json.JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("title", out var t)) dto.Title = t.GetString();
                        if (root.TryGetProperty("address", out var a)) dto.Address = a.GetString();
                        if (root.TryGetProperty("lat", out var lat) && lat.ValueKind != System.Text.Json.JsonValueKind.Null) 
                            dto.Latitude = (decimal)lat.GetDouble();
                        if (root.TryGetProperty("lng", out var lng) && lng.ValueKind != System.Text.Json.JsonValueKind.Null) 
                            dto.Longitude = (decimal)lng.GetDouble();
                    }
                }

                location = new Location
                {
                    PlaceId = dto.PlaceId,
                    Title = dto.Title ?? "Unknown",
                    Address = dto.Address ?? "Unknown",
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude
                };
                await _context.Locations.AddAsync(location);
                await _context.SaveChangesAsync();
            }

            // 2. Prevent duplicate sensor for same location
            var duplicateLocation = await _context.Sensors.AnyAsync(s => s.PlaceId == dto.PlaceId);
            if (duplicateLocation)
                return false;

            // 3. Create sensor
            var sensor = _mapper.Map<Sensor>(dto);

            sensor.InstalledAt = DateTime.UtcNow;
            sensor.CreatedAt = DateTime.UtcNow;

            await _context.Sensors.AddAsync(sensor);
            await _context.SaveChangesAsync();

            // 4. Default reading
            var defaultReading = new SensorReading
            {
                SensorId = sensor.SensorId,
                Status = "Offline",
                WaterLevelCm = 0,
                SignalStrength = "Không kết nối",
                BatteryPercent = 100,
                RecordedAt = DateTime.UtcNow
            };
            await AddSensorReadingAsync(defaultReading);

            return true;
        }

        public async Task<bool> LocationExistsAsync(string placeId)
        {
            return await _context.Locations.AnyAsync(l => l.PlaceId == placeId);
        }

        public async Task<bool> LocationHasSensorAsync(string placeId)
        {
            return await _context.Sensors.AnyAsync(s => s.PlaceId == placeId);
        }

        public async Task<bool> UpdateSensorAsync(int id, UpdateSensorDTO dto)
        {
            var sensor = await _context.Sensors.FindAsync(id);

            if (sensor == null)
                return false;

            if (!string.IsNullOrEmpty(dto.PlaceId))
            {
                var locationExists = await _context.Locations.AnyAsync(l => l.PlaceId == dto.PlaceId);
                if (!locationExists)
                    return false;

                sensor.PlaceId = dto.PlaceId;
            }

            if (dto.TechnicianId.HasValue)
            {
                sensor.TechnicianId = dto.TechnicianId.Value;
            }

            if (!string.IsNullOrEmpty(dto.Specification))
                sensor.Specification = dto.Specification;

            // If sensor code is being changed, ensure uniqueness
            if (!string.IsNullOrEmpty(dto.SensorCode) && dto.SensorCode != sensor.SensorCode)
            {
                var codeExists = await _context.Sensors.AnyAsync(s => s.SensorCode == dto.SensorCode && s.SensorId != sensor.SensorId);
                if (codeExists)
                    return false;

                sensor.SensorCode = dto.SensorCode;
            }

            if (!string.IsNullOrEmpty(dto.SensorName))
                sensor.SensorName = dto.SensorName;

            if (!string.IsNullOrEmpty(dto.Protocol))
                sensor.Protocol = dto.Protocol;

            if (!string.IsNullOrEmpty(dto.SensorType))
                sensor.SensorType = dto.SensorType;

            if (dto.WarningThreshold.HasValue)
                sensor.WarningThreshold = (float?)dto.WarningThreshold.Value;

            if (dto.DangerThreshold.HasValue)
                sensor.DangerThreshold = (float?)dto.DangerThreshold.Value;

            if (dto.MaxLevel.HasValue)
                sensor.MaxLevel = dto.MaxLevel.Value;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSensorAsync(int id)
        {
            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor == null) return false;

            _context.Sensors.Remove(sensor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SensorReading>> GetLatestReadingsForSensorIdsAsync(IEnumerable<int> sensorIds)
        {
            if (sensorIds == null)
                return new List<SensorReading>();

            var ids = sensorIds.Distinct().ToList();

            var latest = await _eventsContext.SensorReadings
                .Where(r => ids.Contains(r.SensorId))
                .GroupBy(r => r.SensorId)
                .Select(g => g.OrderByDescending(r => r.RecordedAt).FirstOrDefault())
                .ToListAsync();

            return latest.Where(r => r != null)!;
        }

        public async Task<IEnumerable<int>> GetAllSensorIdsAsync()
        {
            return await _context.Sensors.Select(s => s.SensorId).ToListAsync();
        }

        public async Task AddSensorReadingAsync(SensorReading reading)
        {
            if (reading == null) return;
            await _eventsContext.SensorReadings.AddAsync(reading);
            await _eventsContext.SaveChangesAsync();
        }

        public async Task PruneSensorReadingsAsync(int sensorId, int maxEntries)
        {
            var readings = await _eventsContext.SensorReadings
                .Where(r => r.SensorId == sensorId)
                .OrderByDescending(r => r.RecordedAt)
                .ToListAsync();

            if (readings.Count <= maxEntries) return;

            var toDelete = readings.Skip(maxEntries).ToList();
            _eventsContext.SensorReadings.RemoveRange(toDelete);
            await _eventsContext.SaveChangesAsync();
        }

        public async Task<double?> GetMaxHistoryLevelForSensorAsync(int sensorId)
        {
            var sensor = await _context.Sensors.FindAsync(sensorId);
            if (sensor == null || string.IsNullOrEmpty(sensor.PlaceId)) return null;

            var maxLevel = await _eventsContext.Histories
                .Where(h => h.LocationId == sensor.PlaceId)
                .MaxAsync(h => (float?)h.MaxWaterLevel);

            return maxLevel;
        }

        public async Task AddHistoryAsync(History history)
        {
            if (history == null) return;
            await _eventsContext.Histories.AddAsync(history);
            await _eventsContext.SaveChangesAsync();
        }
    }
}
