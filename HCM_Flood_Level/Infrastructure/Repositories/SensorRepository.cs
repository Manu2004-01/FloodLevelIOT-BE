using AutoMapper;
using Core.Entities;
using Core.Interfaces;
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
    public class SensorRepository: GenericRepository<Sensor>, ISensor
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
        public async Task<IEnumerable<Sensor>> GetAllWithDetailsAsync()
        {
            return await _context.Sensors
                .Include(s => s.Location)
                    .ThenInclude(l => l.Area)
                .OrderByDescending(s => s.InstalledAt)
                .ToListAsync();
        }

        public async Task<Sensor> GetByIdWithDetailsAsync(int id)
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

        public async Task<bool> UpdateLocationAsync(int sensorId, double latitude, double longitude, string address)
        {
            var sensor = await _context.Sensors
                .Include(s => s.Location)
                .FirstOrDefaultAsync(s => s.SensorId == sensorId);

            if (sensor == null || sensor.Location == null)
                return false;

            sensor.Location.Latitude = latitude;
            sensor.Location.Longitude = longitude;
            sensor.Location.Address = address;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateThresholdAsync(int sensorId, double? minThreshold, double? maxThreshold, string thresholdType)
        {
            var sensor = await _context.Sensors.FindAsync(sensorId);

            if (sensor == null)
                return false;

            //sensor.MinThreshold = minThreshold;
            //sensor.MaxThreshold = maxThreshold;
            //sensor.ThresholdType = thresholdType;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}