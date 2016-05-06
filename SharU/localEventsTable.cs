using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharU
{
    public class localEventsTable
    {
        public string id { get; set; }

        public string HandleName { get; set; }
        public string StartDate { get; set; }
        public string StartTime { get; set; }
        public string EndDate { get; set; }
        public string EndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool pushed { get; set; }
    }
}
