using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly IEventsDBContext _context;
        private readonly ISensorRepository _sensorRepository;

        public HistoryService(IEventsDBContext context, ISensorRepository sensorRepository)
        {
            _context = context;
            _sensorRepository = sensorRepository;
        }

        public async Task ProcessSensorReading(SensorReading reading)
        {
            var sensor = await _sensorRepository.GetAsync(reading.SensorId);
            if (sensor == null) return; // Sensor not found

            // Ensure reading.RecordedAt is treated as UTC
            var recordedAtUtc = DateTime.SpecifyKind(reading.RecordedAt, DateTimeKind.Utc);

            Severity severity = DetermineSeverity(reading.WaterLevelCm, sensor.WarningThreshold, sensor.DangerThreshold);

            var activeHistory = await _context.Histories
                .Where(h => h.LocationId == sensor.PlaceId && h.EndTime == null)
                .FirstOrDefaultAsync();

            if (severity == Severity.Safe)
            {
                if (activeHistory != null)
                {
                    activeHistory.EndTime = recordedAtUtc;
                }
            }
            else // Warning or Danger
            {
                if (activeHistory == null)
                {
                    var newHistory = new History
                    {
                        LocationId = sensor.PlaceId,
                        StartTime = recordedAtUtc,
                        MaxWaterLevel = reading.WaterLevelCm,
                        Severity = severity,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Histories.Add(newHistory);
                }
                else
                {
                    if (reading.WaterLevelCm > activeHistory.MaxWaterLevel)
                    {
                        activeHistory.MaxWaterLevel = reading.WaterLevelCm;
                    }

                    if (IsMoreSevere(severity, activeHistory.Severity))
                    {
                        activeHistory.Severity = severity;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private Severity DetermineSeverity(float? waterLevel, float? warningThreshold, float? dangerThreshold)
        {
            if (!waterLevel.HasValue || !warningThreshold.HasValue || !dangerThreshold.HasValue) return Severity.Safe;
            
            if (waterLevel.Value >= dangerThreshold.Value) return Severity.Danger;
            if (waterLevel.Value >= warningThreshold.Value) return Severity.Warning;
            
            return Severity.Safe;
        }

        private bool IsMoreSevere(Severity newSeverity, Severity oldSeverity)
        {
            return newSeverity > oldSeverity;
        }
    }
}
