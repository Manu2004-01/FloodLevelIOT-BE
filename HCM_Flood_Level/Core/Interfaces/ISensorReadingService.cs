using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;

namespace Core.Interfaces
{
    public interface ISensorReadingService
    {
        Task<SensorReadingDTO?> HandleIncomingData(MqttPayload payload);
    }
}
