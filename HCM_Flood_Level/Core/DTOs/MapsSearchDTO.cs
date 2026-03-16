
namespace Core.DTOs
{
    public class MapsSearchDTO
    {
        public string Query { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public int Zoom { get; set; } = 14;
        public string Hl { get; set; } = "vi";
    }
}
