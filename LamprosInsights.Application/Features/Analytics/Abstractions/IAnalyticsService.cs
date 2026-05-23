using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Application.Features.Analytics.Abstractions
{
    public interface IAnalyticsService
    {

        Task<string> GenerateSqlAsync(
            string question,
            CancellationToken cancellationToken = default);
    }
}
