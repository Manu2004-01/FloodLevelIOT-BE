using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IEventsDBContext
    {
        DbSet<SensorReading> SensorReadings { get; set; }
        DbSet<History> Histories { get; set; }
        DbSet<Report> Reports { get; set; }
        DbSet<Sensor> Sensors { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
