using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Domain.Analytics
{
    public class AnalyticsQueries
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string GeneratedSql { get; set; }
        public decimal ExecutionTimeMs { get; set; }
        public bool Success { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}
