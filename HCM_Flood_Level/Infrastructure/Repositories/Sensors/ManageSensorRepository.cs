using AutoMapper;
using Core.DTOs.Admin;
using Core.DTOs.Sensor;
using Core.Entities;
using Core.Interfaces.Sensors;
using Core.Sharing;
using Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Sensors
{
    public class ManageSensorRepository : GenericRepository<Sensor>, IManageSensorRepository
    {
        private readonly ManageDBContext _context;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;

        public ManageSensorRepository(ManageDBContext context, IFileProvider fileProvider, IMapper mapper) : base(context)
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
            var sensorCodeExists = await _context.Sensors.AnyAsync(s => s.SensorCode == dto.SensorCode);
            if (sensorCodeExists)
                return false;

            var locationExists = await _context.Locations.AnyAsync(l => l.LocationId == dto.LocationId);
            if (!locationExists)
                return false;

            // Ensure the installer (staff) exists to avoid FK constraint errors
            if (dto.InstalledBy <= 0 || !await _context.Staffs.AnyAsync(s => s.StaffId == dto.InstalledBy))
                return false;

            var sensor = _mapper.Map<Sensor>(dto);

            sensor.InstalledAt = DateTime.UtcNow;
            sensor.CreatedAt = DateTime.UtcNow;

            await _context.Sensors.AddAsync(sensor);
            await _context.SaveChangesAsync();
            return true;    
        }

        public async Task<bool> UpdateSensorAsync(int id, UpdateSensorDTO dto)
        {
            var sensor = await _context.Sensors.FindAsync(id);

            if (sensor == null)
                return false;

            // Location change: validate existence
            if (dto.LocationId.HasValue)
            {
                var locationExists = await _context.Locations.AnyAsync(l => l.LocationId == dto.LocationId.Value);
                if (!locationExists)
                    return false;

                sensor.LocationId = dto.LocationId.Value;
            }

            // InstalledBy change: validate staff exists
            if (dto.InstalledBy.HasValue)
            {
                var staffExists = await _context.Staffs.AnyAsync(s => s.StaffId == dto.InstalledBy.Value);
                if (!staffExists)
                    return false;

                sensor.InstalledBy = dto.InstalledBy.Value;
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

            if (dto.MinThreshold.HasValue)
                sensor.WarningThreshold = dto.MinThreshold.Value;

            if (dto.MaxThreshold.HasValue)
                sensor.DangerThreshold = dto.MaxThreshold.Value;

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
    }
}