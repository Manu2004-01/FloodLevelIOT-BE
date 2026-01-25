using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class FloodLevel
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string Color { get; set; }
    }
}
