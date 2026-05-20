using LamprosInsights.Application.Features.Analytics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Application.Features.Analytics.Services
{
    public class AnalyticsService : IAnalyticsService

    {

        private readonly ISchemaProvider _schemaProvider;

        public AnalyticsService(ISchemaProvider schemaProvider)
        {
            _schemaProvider = schemaProvider;
        }

        public async Task<string> GetSchemaAsync(CancellationToken cancellationToken = default)
        {
            return await _schemaProvider.GetSchemaContextAsync(cancellationToken);
        }
    }
}
