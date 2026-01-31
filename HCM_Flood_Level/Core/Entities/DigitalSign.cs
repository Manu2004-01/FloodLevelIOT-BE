using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class DigitalSign
    {
        public int SignId { get; set; }
        public string SignCode { get; set; }
        public string Status { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }
    }
}
