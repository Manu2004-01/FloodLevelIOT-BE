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

        public HistoryService(IEventsDBContext context)
        {
            _context = context;
        }

        public async Task ProcessSensorReading(SensorReading reading)
        {
            var sensor = await _context.Sensors.FindAsync(reading.SensorId);
            if (sensor == null) return; // Sensor not found

            Severity severity = DetermineSeverity(reading.WaterLevelCm, (float)sensor.WarningThreshold, (float)sensor.DangerThreshold);

            var activeHistory = await _context.Histories
                .Where(h => h.LocationId == sensor.PlaceId && h.EndTime == null)
                .FirstOrDefaultAsync();

            if (severity == Severity.Safe)
            {
                if (activeHistory != null)
                {
                    activeHistory.EndTime = reading.RecordedAt;
                }
            }
            else // Warning or Danger
            {
                if (activeHistory == null)
                {
                    var newHistory = new History
                    {
                        LocationId = sensor.PlaceId,
                        StartTime = reading.RecordedAt,
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
