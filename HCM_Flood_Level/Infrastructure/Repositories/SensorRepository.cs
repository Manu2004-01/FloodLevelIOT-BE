using AutoMapper;
using Core.DTOs.Admin;
using Core.DTOs.Sensor;
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
    public class SensorRepository : GenericRepository<Sensor>, ISensor
    {
        private readonly ManageDBContext _context;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;

        public SensorRepository(ManageDBContext context, IFileProvider fileProvider, IMapper mapper) : base(context)
        {
            _context = context;
            _fileProvider = fileProvider;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Sensor>> GetAllSensorsAsync(EntityParam param)
        {
            var query = _context.Sensors
                .Include(s => s.Location)
                    .ThenInclude(l => l.Area)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                query = query.Where(s =>
                    s.SensorCode.Contains(param.Search) ||
                    s.SensorName.Contains(param.Search) ||
                    s.SensorType.Contains(param.Search) ||
                    s.Location.LocationName.Contains(param.Search)
                );
            }

            // Pagination
            return await query
                .OrderByDescending(s => s.InstalledAt)
                .Skip((param.Pagenumber - 1) * param.Pagesize)
                .Take(param.Pagesize)
                .ToListAsync();
        }

        public async Task<int> CountAsync(string? search = null)
        {
            var query = _context.Sensors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.SensorCode.Contains(search) ||
                    s.SensorName.Contains(search) ||
                    s.SensorType.Contains(search)
                );
            }

            return await query.CountAsync();
        }

        public async Task<Sensor> GetSensorByIdAsync(int id)
        {
            return await _context.Sensors
                .Include(s => s.Location)
                    .ThenInclude(l => l.Area)
                .FirstOrDefaultAsync(s => s.SensorId == id);
        }

        public async Task<bool> SensorCodeExistsAsync(string sensorCode, int? excludeId = null)
        {
            var query = _context.Sensors.Where(s => s.SensorCode == sensorCode);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.SensorId != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> AddNewSensorAsync(CreateSensorDTO dto)
        {
            try
            {
                // Check if sensor code already exists
                if (await SensorCodeExistsAsync(dto.SensorCode))
                    return false;

                // Check if location exists
                var locationExists = await _context.Locations.AnyAsync(l => l.LocationId == dto.LocationId);
                if (!locationExists)
                    return false;

                var sensor = new Sensor
                {
                    SensorCode = dto.SensorCode,
                    SensorName = dto.SensorName,
                    SensorType = dto.SensorType,
                    SensorStatus = dto.SensorStatus,
                    InstalledAt = DateTime.SpecifyKind(dto.InstalledAt, DateTimeKind.Utc),
                    LocationId = dto.LocationId,
                    MinThreshold = dto.MinThreshold ?? 0,
                    MaxThreshold = dto.MaxThreshold ?? 0,
                    ThresholdType = dto.ThresholdType ?? string.Empty
                };

                await _context.Sensors.AddAsync(sensor);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSensorAsync(int id, UpdateSensorDTO dto)
        {
            try
            {
                var sensor = await _context.Sensors.FindAsync(id);
                if (sensor == null)
                    return false;

                // Update only provided fields
                if (!string.IsNullOrEmpty(dto.SensorName))
                    sensor.SensorName = dto.SensorName;

                if (!string.IsNullOrEmpty(dto.SensorType))
                    sensor.SensorType = dto.SensorType;

                if (!string.IsNullOrEmpty(dto.SensorStatus))
                    sensor.SensorStatus = dto.SensorStatus;

                if (dto.LocationId.HasValue)
                {
                    // Check if location exists
                    var locationExists = await _context.Locations.AnyAsync(l => l.LocationId == dto.LocationId.Value);
                    if (!locationExists)
                        return false;

                    sensor.LocationId = dto.LocationId.Value;
                }

                // Ensure InstalledAt has UTC kind before saving to PostgreSQL (Npgsql requires UTC for timestamptz)
                sensor.InstalledAt = DateTime.SpecifyKind(sensor.InstalledAt, DateTimeKind.Utc);

                _context.Sensors.Update(sensor);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateLocationAsync(int sensorId, UpdateLocationDTO dto)
        {
            try
            {
                var sensor = await _context.Sensors
                    .Include(s => s.Location)
                    .FirstOrDefaultAsync(s => s.SensorId == sensorId);

                if (sensor == null || sensor.Location == null)
                    return false;

                sensor.Location.Latitude = dto.Latitude;
                sensor.Location.Longitude = dto.Longitude;
                sensor.Location.Address = dto.Address;

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateThresholdAsync(int sensorId, UpdateThresholdDTO dto)
        {
            try
            {
                var sensor = await _context.Sensors.FindAsync(sensorId);
                if (sensor == null)
                    return false;

                if (dto.MinThreshold.HasValue)
                    sensor.MinThreshold = dto.MinThreshold.Value;

                if (dto.MaxThreshold.HasValue)
                    sensor.MaxThreshold = dto.MaxThreshold.Value;

                if (!string.IsNullOrWhiteSpace(dto.ThresholdType))
                    sensor.ThresholdType = dto.ThresholdType;

                _context.Sensors.Update(sensor);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSensorAsync(int id)
        {
            try
            {
                var sensor = await _context.Sensors.FindAsync(id);
                if (sensor == null)
                    return false;

                _context.Sensors.Remove(sensor);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}