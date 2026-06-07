using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Application.Features.Analytics.Dtos
{
    public class GenerateSqlResponse
    {
        public string Sql { get; set; } = string.Empty;

        public bool IsValid { get; set; }

        public List<string> ValidationErrors { get; set; } = [];
        public string? ErrorMessage { get; set; }

        public AnalyticsQueryResult? Result { get; set; }
    }
}
