using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Area
    {
        public int AreaId { get; set; }
        public string AreaName { get; set; }
        // Navigation property
        public ICollection<Location> Locations { get; set; }
    }
}
