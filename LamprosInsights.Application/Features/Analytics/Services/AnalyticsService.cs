using LamprosInsights.Application.Features.Analytics.Abstractions;
using LamprosInsights.Application.Features.Analytics.Dtos;
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
        private readonly ISqlValidator _sqlValidator;

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
            _sqlValidator = sqlValidator;
        }

        public async Task<GenerateSqlResponse> GenerateSqlAsync(
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

            var validation = _sqlValidator.Validate(sql);
            return new GenerateSqlResponse
            {
                Sql = sql,
                IsValid = validation.IsValid,
                ValidationErrors = validation.Errors
            };
        }
    }
}
