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

        public ManageSensorRepository(ManageDBContext context, EventsDBContext eventsContext, IFileProvider fileProvider, IMapper mapper) : base(context)
        {
            _context = context;
            _eventsContext = eventsContext;
            _fileProvider = fileProvider;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Sensor>> GetAllSensorsAsync(EntityParam param)
        {
            var query = _context.Sensors
                .Include(s => s.Location)
                    .ThenInclude(l => l.Area)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                query = query.Where(s =>
                    s.SensorCode.Contains(param.Search) ||
                    s.SensorName.Contains(param.Search) ||
                    s.SensorType.Contains(param.Search) ||
                    s.Location.LocationName.Contains(param.Search)
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
            var locationExists = await _context.Locations.AnyAsync(l => l.LocationId == dto.LocationId);
            if (!locationExists)
                return false;

            var duplicateLocation = await _context.Sensors.AnyAsync(s => s.LocationId == dto.LocationId);
            if (duplicateLocation)
                return false;

            var sensor = _mapper.Map<Sensor>(dto);

            sensor.InstalledAt = DateTime.UtcNow;
            sensor.CreatedAt = DateTime.UtcNow;

            await _context.Sensors.AddAsync(sensor);
            await _context.SaveChangesAsync();

            // Sau khi tạo sensor, tạo bản ghi SensorReading mặc định
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

        public async Task<bool> LocationExistsAsync(int locationId)
        {
            return await _context.Locations.AnyAsync(l => l.LocationId == locationId);
        }

        public async Task<bool> LocationHasSensorAsync(int locationId)
        {
            return await _context.Sensors.AnyAsync(s => s.LocationId == locationId);
        }

        public async Task<bool> UpdateSensorAsync(int id, UpdateSensorDTO dto)
        {
            var sensor = await _context.Sensors.FindAsync(id);

            if (sensor == null)
                return false;

            if (dto.LocationId.HasValue)
            {
                var locationExists = await _context.Locations.AnyAsync(l => l.LocationId == dto.LocationId.Value);
                if (!locationExists)
                    return false;

                sensor.LocationId = dto.LocationId.Value;
            }

            // No InstalledBy property in Sensor, so skip this block

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

            if (dto.MinThreshold.HasValue)
                sensor.WarningThreshold = (float?)dto.MinThreshold.Value;

            if (dto.MaxThreshold.HasValue)
                sensor.DangerThreshold = (float?)dto.MaxThreshold.Value;

            if (dto.MaxLevel.HasValue)
                sensor.MaxLevel = dto.MaxLevel.Value;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSensorAsync(int id)
        {
            var sensor = await _context.Sensors.FindAsync(id);

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

        public async Task<double?> GetMaxFloodEventLevelForSensorAsync(int sensorId)
        {
            var evt = await _eventsContext.FloodEvents
                .Where(f => f.SensorId == sensorId)
                .OrderByDescending(f => f.MaxWaterLevel)
                .FirstOrDefaultAsync();

            return evt?.MaxWaterLevel;
        }

        public async Task AddFloodEventAsync(FloodEvent floodEvent)
        {
            if (floodEvent == null) return;
            floodEvent.CreatedAt = DateTime.UtcNow;
            await _eventsContext.FloodEvents.AddAsync(floodEvent);
            await _eventsContext.SaveChangesAsync();
        }
    }
}