using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class AlertLog
    {
        public int LogId { get; set; }
        public string Channel { get; set; }
        public DateTime SentAt { get; set; }
        public string LogStatus { get; set; }

        public int AlertId { get; set; }
        public Alert Alert { get; set; }
    }
}
