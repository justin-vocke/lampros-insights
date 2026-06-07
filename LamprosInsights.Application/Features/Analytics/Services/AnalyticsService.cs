using LamprosInsights.Application.Features.Analytics.Abstractions;
using LamprosInsights.Application.Features.Analytics.Dtos;
using LamprosInsights.Application.Features.Analytics.Prompts;
using LamprosInsights.Application.Features.Analytics.Validation;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AnalyticsService> _logger;
        private readonly IAIProvider _aiProvider;
        private readonly ISqlValidator _sqlValidator;
        private readonly ISqlExecutor _sqlExecutor;

        private readonly AnalyticsPromptBuilder _promptBuilder;

        public AnalyticsService(
            ISchemaProvider schemaProvider,
            IAIProvider aiProvider,
            AnalyticsPromptBuilder promptBuilder,
            ISqlValidator sqlValidator,
            ILogger<AnalyticsService> logger,
            ISqlExecutor sqlExecutor)
        {
            _schemaProvider = schemaProvider;
            _aiProvider = aiProvider;
            _promptBuilder = promptBuilder;
            _sqlValidator = sqlValidator;
            _logger = logger;
            _sqlExecutor = sqlExecutor;
        }

        public async Task<GenerateSqlResponse> GenerateSqlAsync(
            string question,
            CancellationToken cancellationToken = default)
        {
            try
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

                if (!validation.IsValid)
                {
                    return new GenerateSqlResponse
                    {
                        Sql = sql,
                        IsValid = false,
                        ValidationErrors = validation.Errors
                    };
                }
                var result = await _sqlExecutor.ExecuteAsync(sql);
                return new GenerateSqlResponse
                {
                    Sql = sql,
                    IsValid = validation.IsValid,
                    ValidationErrors = validation.Errors,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                //TODO: Add more robust logging
                _logger.LogError(
                    ex,
                    "Error generating analytics query for question: {Question}",
                    question);

                throw;
            }
        }
    }
}
