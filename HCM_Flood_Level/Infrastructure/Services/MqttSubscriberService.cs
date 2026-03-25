using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;
using Core.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Core.Interfaces;

public class MqttSubscriberService
{
    private readonly IServiceProvider _serviceProvider;

    public MqttSubscriberService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync()
    {
        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("broker.hivemq.com", 1883)
            .Build();

        client.ApplicationMessageReceivedAsync += async e =>
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            Console.WriteLine("Received: " + payload);

            var data = JsonSerializer.Deserialize<MqttPayload>(payload);

            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ISensorReadingService>();

            await service.HandleIncomingData(data);
        };

        await client.ConnectAsync(options);

        await client.SubscribeAsync("flood/+/telemetry");

        Console.WriteLine("MQTT Connected & Subscribed");
    }
}