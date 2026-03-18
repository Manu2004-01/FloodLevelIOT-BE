using Core.Entities;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IHistoryService
    {
        Task ProcessSensorReading(SensorReading reading);
    }
}
