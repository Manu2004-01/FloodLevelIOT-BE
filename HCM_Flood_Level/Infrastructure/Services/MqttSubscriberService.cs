using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;
using Core.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Core.Interfaces;

using Microsoft.Extensions.Configuration;

public class MqttSubscriberService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    // Buffer giữ 10 MQTT message gần nhất để debug
    private static readonly LinkedList<string> _recentMessages = new();
    private static readonly object _lock = new();
    private const int MaxBufferSize = 10;

    public MqttSubscriberService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// Lấy danh sách 10 MQTT message gần nhất (mới nhất trước).
    /// </summary>
    public static List<string> GetRecentMessages()
    {
        lock (_lock)
        {
            return _recentMessages.ToList();
        }
    }

    public async Task StartAsync()
    {
        var mqttConfig = _configuration.GetSection("Mqtt");
        var host = mqttConfig["Host"];
        var port = int.Parse(mqttConfig["Port"] ?? "8883");
        var username = mqttConfig["Username"];
        var password = mqttConfig["Password"];
        var topic = mqttConfig["Topic"] ?? "flood/+/telemetry";

        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();

        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port);

        if (!string.IsNullOrEmpty(username))
        {
            optionsBuilder.WithCredentials(username, password);
        }

        // HiveMQ Cloud requires TLS
        if (port == 8883)
        {
            optionsBuilder.WithTls(new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                IgnoreCertificateChainErrors = false,
                IgnoreCertificateRevocationErrors = false,
                AllowUntrustedCertificates = false
            });
        }

        var options = optionsBuilder.Build();

        client.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Console.WriteLine($"[MQTT Received] Topic: {e.ApplicationMessage.Topic} | Payload: {payload}");

                // Lưu message vào buffer debug
                lock (_lock)
                {
                    _recentMessages.AddFirst($"[{DateTime.UtcNow:HH:mm:ss}] {payload}");
                    if (_recentMessages.Count > MaxBufferSize)
                        _recentMessages.RemoveLast();
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<MqttPayload>(payload, options);
                
                if (data != null && !string.IsNullOrEmpty(data.DeviceId))
                {
                    using var scope = _serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<ISensorReadingService>();
                    await service.HandleIncomingData(data);
                }
                else
                {
                    Console.WriteLine("[MQTT Error] Deserialization failed or DeviceId is empty.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing MQTT message: {ex.Message}");
            }
        };

        client.DisconnectedAsync += async e =>
        {
            Console.WriteLine("MQTT Disconnected. Retrying in 5 seconds...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            try
            {
                await client.ConnectAsync(options);
            }
            catch
            {
                Console.WriteLine("MQTT Reconnection failed.");
            }
        };

        try
        {
            await client.ConnectAsync(options);
            await client.SubscribeAsync(topic);
            Console.WriteLine($"MQTT Connected to {host}:{port} and Subscribed to {topic}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MQTT Connection failed: {ex.Message}");
        }
    }
}