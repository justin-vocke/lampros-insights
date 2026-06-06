using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Application.Features.Analytics.Dtos
{
    public class AnalyticsQueryResult
    {
        public List<ColumnDefinition> Columns { get; set; } = [];

        public List<Dictionary<string, object?>> Rows { get; set; } = [];

        public int RowCount { get; set; }

        public long ExecutionTimeMs { get; set; }
    }
}
