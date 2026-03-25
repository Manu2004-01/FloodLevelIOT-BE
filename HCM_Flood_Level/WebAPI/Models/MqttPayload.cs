namespace WebAPI.Models
{
    public class MqttPayload
    {
        public string DeviceId { get; set; }
        public float DistanceCm { get; set; }
        public float Height { get; set; }
    }
}
