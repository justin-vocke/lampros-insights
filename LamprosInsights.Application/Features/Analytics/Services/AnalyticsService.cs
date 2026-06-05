using LamprosInsights.Application.Features.Analytics.Abstractions;
using LamprosInsights.Application.Features.Analytics.Prompts;
using LamprosInsights.Application.Features.Analytics.Validation;
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
        private readonly ISqlValidator sqlValidator;

        private readonly AnalyticsPromptBuilder _promptBuilder;

        public AnalyticsService(
            ISchemaProvider schemaProvider,
            IAIProvider aiProvider,
            AnalyticsPromptBuilder promptBuilder,
            ISqlValidator sqlValidator)
        {
            _schemaProvider = schemaProvider;
            _aiProvider = aiProvider;
            _promptBuilder = promptBuilder;
            this.sqlValidator = sqlValidator;
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
