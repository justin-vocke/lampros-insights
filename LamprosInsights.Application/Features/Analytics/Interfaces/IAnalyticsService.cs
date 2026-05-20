using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Application.Features.Analytics.Interfaces
{
    public interface IAnalyticsService
    {
        Task<string> GetSchemaAsync(
            CancellationToken cancellationToken = default);
    }
}
