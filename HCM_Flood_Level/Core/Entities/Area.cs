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
        // Description removed to match DB schema (only AreaId, AreaName)
    }
}
