using LamprosInsights.Application.Features.Analytics.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Application.Features.Analytics.Abstractions
{
    public interface IAnalyticsService
    {

        Task<GenerateSqlResponse> GenerateSqlAsync(
            string question,
            CancellationToken cancellationToken = default);
    }
}
