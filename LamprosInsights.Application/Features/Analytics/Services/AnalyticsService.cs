using LamprosInsights.Application.Features.Analytics.Abstractions;
using LamprosInsights.Application.Features.Analytics.Prompts;
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

        private readonly IAIProvider _aiProvider;

        private readonly AnalyticsPromptBuilder _promptBuilder;

        public AnalyticsService(
            ISchemaProvider schemaProvider,
            IAIProvider aiProvider,
            AnalyticsPromptBuilder promptBuilder)
        {
            _schemaProvider = schemaProvider;
            _aiProvider = aiProvider;
            _promptBuilder = promptBuilder;
        }

        public async Task<string> GenerateSqlAsync(
            string question,
            CancellationToken cancellationToken = default)
        {
            var schema =
                await _schemaProvider
                    .GetSchemaContextAsync(cancellationToken);

            var prompt =
                _promptBuilder.BuildSqlGenerationPrompt(
                    schema,
                    question);

            var sql =
                await _aiProvider.GenerateSqlAsync(
                    prompt,
                    cancellationToken);

            return sql;
        }
    }
}
