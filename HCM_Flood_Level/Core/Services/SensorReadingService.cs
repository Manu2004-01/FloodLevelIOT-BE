using Core.DTOs;
using Core.Entities;
using Core.Interfaces;

public class SensorReadingService : ISensorReadingService
{
    private readonly ISensorRepository _sensorRepo;
    private readonly ISensorReadingRepository _readingRepo;
    private readonly IHistoryService _historyService;

    public SensorReadingService(
        ISensorRepository sensorRepo,
        ISensorReadingRepository readingRepo,
        IHistoryService historyService)
    {
        _sensorRepo = sensorRepo;
        _readingRepo = readingRepo;
        _historyService = historyService;
    }

    public async Task HandleIncomingData(MqttPayload payload)
    {
        var sensor = await _sensorRepo.GetByDeviceId(payload.DeviceId);
        if (sensor == null)
        {
            Console.WriteLine($"[MQTT Error] Sensor with DeviceId '{payload.DeviceId}' not found in database.");
            return;
        }

        // Use waterCm from ESP32 payload or calculate if necessary
        float waterLevel = payload.WaterCm;

        // Determine status based on ESP32 'level' field or thresholds
        string status = payload.Level ?? "Online";

        var reading = new SensorReading
        {
            SensorId = sensor.SensorId,
            WaterLevelCm = waterLevel,
            Status = status,
            RecordedAt = DateTime.UtcNow,
            SignalStrength = "Ổn định", // Default for active MQTT connection
            BatteryPercent = 100 // ESP32 currently doesn't provide battery in this payload
        };

        await _readingRepo.AddAsync(reading);
        
        // Process this reading to update History
        await _historyService.ProcessSensorReading(reading);
        
        Console.WriteLine($"[MQTT Processed] Device: {payload.DeviceId} | Level: {waterLevel}cm | Status: {status}");
    }
}