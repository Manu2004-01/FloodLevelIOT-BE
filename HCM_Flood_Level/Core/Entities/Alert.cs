using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Alert
    {
        public long AlertId { get; set; }
        public int LocationId { get; set; }
        public int LevelId { get; set; }
        public string AlertMessage { get; set; }
        public DateTime IssuedAt { get; set; }
        // Navigation property
        public Location Location { get; set; }
        public ICollection<AlertLog> AlertLogs { get; set; }
    }
}
