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
    public class MaintenanceScheduleRepository : GenericRepository<MaintenanceSchedule>, IMaintenanceScheduleRepository
    {
        private readonly ManageDBContext _context;
        private readonly IFileProvider _fileProvider;
        private readonly IMapper _mapper;

        public MaintenanceScheduleRepository(ManageDBContext context, IFileProvider fileProvider, IMapper mapper) : base(context)
        {
            _context = context;
            _fileProvider = fileProvider;
            _mapper = mapper;
        }

        public async Task<bool> AddNewScheduleAsync(CreateMaintenanceScheduleDTO dto)
        {
            var sensorIdExist = await _context.MaintenanceSchedules.AnyAsync(s => s.SensorId == dto.SensorId);
            if (sensorIdExist) return false;

            var schedule = _mapper.Map<MaintenanceSchedule>(dto);

            schedule.Status = "Scheduled";
            schedule.CreatedAt = DateTime.UtcNow;

            await _context.MaintenanceSchedules.AddAsync(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MaintenanceSchedule>> GetAllSchedulesAsync(EntityParam entityParam)
        {
            var query = _context.MaintenanceSchedules
                .Include(u => u.Sensor)
                .Include(u => u.AssignedTechnician)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityParam.ScheduleStatus))
            {
                query = query.Where(s => s.Status.ToLower() == entityParam.ScheduleStatus.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(entityParam.ScheduleType))
            {
                query = query.Where(s => s.Status.ToLower() == entityParam.ScheduleType.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(entityParam.ScheduleMode))
            {
                query = query.Where(s => s.Status.ToLower() == entityParam.ScheduleMode.ToLower());
            }

            query = query.OrderByDescending(s => s.CreatedAt)
                         .Skip((entityParam.Pagenumber - 1) * entityParam.Pagesize)
                         .Take(entityParam.Pagesize);

            return await query.ToListAsync();
        }
    }
}
