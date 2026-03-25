using Core.DTOs;
using Core.Entities;
using Core.Interfaces;

public class SensorReadingService : ISensorReadingService
{
    private readonly ISensorRepository _sensorRepo;
    private readonly ISensorReadingRepository _readingRepo;

    public SensorReadingService(
        ISensorRepository sensorRepo,
        ISensorReadingRepository readingRepo)
    {
        _sensorRepo = sensorRepo;
        _readingRepo = readingRepo;
    }

    public async Task HandleIncomingData(MqttPayload payload)
    {
        var sensor = await _sensorRepo.GetByDeviceId(payload.DeviceId);
        if (sensor == null) return;

        float waterLevel = payload.Height - payload.DistanceCm;

        string status = "OK";

        if (waterLevel >= sensor.DangerThreshold)
            status = "DANGER";
        else if (waterLevel >= sensor.WarningThreshold)
            status = "WARN";

        var reading = new SensorReading
        {
            SensorId = sensor.SensorId,
            WaterLevelCm = waterLevel,
            Status = status,
            RecordedAt = DateTime.UtcNow
        };

        await _readingRepo.AddAsync(reading);
    }
}