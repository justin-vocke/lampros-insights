using LamprosInsights.Application.Features.Analytics.Abstractions;
using LamprosInsights.Application.Features.Analytics.Dtos;
using LamprosInsights.Application.Features.Analytics.Models;
using LamprosInsights.Application.Features.Analytics.Validation;

namespace LamprosInsights.Application.Tests.Fakes;

// Hand-written test doubles. The abstractions are small enough that a
// mocking library isn't needed; each fake records what it was called with.

internal sealed class FakeSchemaProvider : ISchemaProvider
{
    public string SchemaContext { get; set; } = "Table: Customers (Id int, Name nvarchar)";
    public CancellationToken? ReceivedToken { get; private set; }

    public Task<string> GetSchemaContextAsync(CancellationToken cancellationToken = default)
    {
        ReceivedToken = cancellationToken;
        return Task.FromResult(SchemaContext);
    }

    public Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new DatabaseSchema());
}

internal sealed class FakeAIProvider : IAIProvider
{
    public string SqlToReturn { get; set; } = "SELECT TOP 100 Name FROM Customers";
    public Exception? ExceptionToThrow { get; set; }
    public string? ReceivedPrompt { get; private set; }
    public CancellationToken? ReceivedToken { get; private set; }

    public Task<string> GenerateSqlAsync(string prompt, CancellationToken cancellationToken = default)
    {
        ReceivedPrompt = prompt;
        ReceivedToken = cancellationToken;

        if (ExceptionToThrow is not null)
            throw ExceptionToThrow;

        return Task.FromResult(SqlToReturn);
    }
}

internal sealed class FakeSqlValidator : ISqlValidator
{
    public List<string> ErrorsToReturn { get; set; } = [];
    public string? ReceivedSql { get; private set; }

    public SqlValidationResult Validate(string sql)
    {
        ReceivedSql = sql;
        var result = new SqlValidationResult();
        result.Errors.AddRange(ErrorsToReturn);
        return result;
    }
}

internal sealed class FakeSqlExecutor : ISqlExecutor
{
    public AnalyticsQueryResult ResultToReturn { get; set; } = new();
    public Exception? ExceptionToThrow { get; set; }
    public List<string> ExecutedSql { get; } = [];

    public Task<AnalyticsQueryResult> ExecuteAsync(string sql, CancellationToken cancellationToken = default)
    {
        ExecutedSql.Add(sql);

        if (ExceptionToThrow is not null)
            throw ExceptionToThrow;

        return Task.FromResult(ResultToReturn);
    }
}
