using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class AreaRepository : GenericRepository<Area>, IAreaRepository
    {
        private readonly ManageDBContext _manageContext;
        private readonly EventsDBContext _eventsContext;

        public AreaRepository(ManageDBContext context, EventsDBContext eventsContext) : base(context)
        {
            _manageContext = context;
            _eventsContext = eventsContext;
        }

        public async Task<IReadOnlyList<AreaDTO>> GetAreaSensorReadingsAsync(int? areaId = null, CancellationToken cancellationToken = default)
        {
            var query =
                from s in _manageContext.Sensors.AsNoTracking()
                join l in _manageContext.Locations.AsNoTracking() on s.PlaceId equals l.PlaceId
                join a in _manageContext.Areas.AsNoTracking() on l.AreaId equals a.AreaId
                where areaId == null || a.AreaId == areaId
                select new { Area = a, Location = l, Sensor = s };

            var rows = await query.ToListAsync(cancellationToken);
            if (rows.Count == 0)
                return new List<AreaDTO>();

            var sensorIds = rows.Select(r => r.Sensor.SensorId).Distinct().ToList();
            var readingBySensor = await GetLatestReadingBySensorIdAsync(sensorIds, cancellationToken);

            var result = new List<AreaDTO>(rows.Count);
            foreach (var r in rows)
            {
                readingBySensor.TryGetValue(r.Sensor.SensorId, out var rd);
                result.Add(new AreaDTO
                {
                    AreaId = r.Area.AreaId,
                    AreaName = r.Area.AreaName ?? string.Empty,
                    Title = r.Location.Title,
                    Address = r.Location.Address,
                    SensorId = r.Sensor.SensorId,
                    SensorName = r.Sensor.SensorName ?? string.Empty,
                    ReadingId = rd?.ReadingId ?? 0L,
                    WaterLevelCm = rd?.WaterLevelCm ?? 0f
                });
            }

            return result;
        }

        private async Task<Dictionary<int, SensorReading>> GetLatestReadingBySensorIdAsync(
            List<int> sensorIds,
            CancellationToken cancellationToken)
        {
            if (sensorIds.Count == 0)
                return new Dictionary<int, SensorReading>();

            var latest = await _eventsContext.SensorReadings
                .AsNoTracking()
                .Where(r => sensorIds.Contains(r.SensorId))
                .GroupBy(r => r.SensorId)
                .Select(g => g.OrderByDescending(r => r.RecordedAt).FirstOrDefault())
                .ToListAsync(cancellationToken);

            return latest
                .Where(r => r != null)
                .ToDictionary(r => r!.SensorId, r => r!);
        }
    }
}
