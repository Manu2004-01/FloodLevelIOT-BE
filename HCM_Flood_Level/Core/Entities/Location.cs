using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Location
    {
        public int PlaceId { get; set; }
        public int AreaId { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        // Navigation properties
        public Area Area { get; set; }
        public ICollection<Sensor> Sensors { get; set; }
    }
}
