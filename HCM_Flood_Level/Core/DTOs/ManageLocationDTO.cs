using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class ManageLocationDTO
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
    }

    public class LocationDetailDTO
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int AreaId { get; set; }
        public string AreaName { get; set; }
    }
}
