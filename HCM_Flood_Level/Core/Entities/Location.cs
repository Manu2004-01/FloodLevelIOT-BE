using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Location
    {
        public int LocationId { get; set; }
        public int AreaId { get; set; }
        public Area Area { get; set; }
        public string LocationName { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string RoadName { get; set; }
        // Navigation property
        public ICollection<Sensor> Sensors { get; set; }
    }
}
