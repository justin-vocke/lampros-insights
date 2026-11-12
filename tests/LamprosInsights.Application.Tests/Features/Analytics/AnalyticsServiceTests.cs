using LamprosInsights.Application.Features.Analytics.Dtos;
using LamprosInsights.Application.Features.Analytics.Prompts;
using LamprosInsights.Application.Features.Analytics.Services;
using LamprosInsights.Application.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace LamprosInsights.Application.Tests.Features.Analytics;

public class AnalyticsServiceTests
{
    private readonly FakeSchemaProvider _schemaProvider = new();
    private readonly FakeAIProvider _aiProvider = new();
    private readonly FakeSqlValidator _validator = new();
    private readonly FakeSqlExecutor _executor = new();

    private AnalyticsService CreateService() => new(
        _schemaProvider,
        _aiProvider,
        new AnalyticsPromptBuilder(),
        _validator,
        NullLogger<AnalyticsService>.Instance,
        _executor);

    [Fact]
    public async Task GenerateSqlAsync_ValidSql_ExecutesAndReturnsResult()
    {
        var expected = new AnalyticsQueryResult { RowCount = 3 };
        _executor.ResultToReturn = expected;

        var response = await CreateService().GenerateSqlAsync("How many customers?");

        Assert.True(response.IsValid);
        Assert.Empty(response.ValidationErrors);
        Assert.Equal(_aiProvider.SqlToReturn, response.Sql);
        Assert.Same(expected, response.Result);
        Assert.Equal([_aiProvider.SqlToReturn], _executor.ExecutedSql);
    }

    [Fact]
    public async Task GenerateSqlAsync_InvalidSql_IsNotExecuted()
    {
        _aiProvider.SqlToReturn = "DROP TABLE Customers";
        _validator.ErrorsToReturn = ["SQL must begin with SELECT statement"];

        var response = await CreateService().GenerateSqlAsync("Delete everything");

        Assert.False(response.IsValid);
        Assert.Equal(["SQL must begin with SELECT statement"], response.ValidationErrors);
        Assert.Equal("DROP TABLE Customers", response.Sql);
        Assert.Null(response.Result);
        Assert.Empty(_executor.ExecutedSql);
    }

    [Fact]
    public async Task GenerateSqlAsync_ValidatesTheSqlReturnedByTheAIProvider()
    {
        await CreateService().GenerateSqlAsync("Top customers");

        Assert.Equal(_aiProvider.SqlToReturn, _validator.ReceivedSql);
    }

    [Fact]
    public async Task GenerateSqlAsync_SendsSchemaAndQuestionToAIProvider()
    {
        _schemaProvider.SchemaContext = "Table: Orders (Id int, Total decimal)";

        await CreateService().GenerateSqlAsync("What is total revenue?");

        Assert.NotNull(_aiProvider.ReceivedPrompt);
        Assert.Contains("Table: Orders (Id int, Total decimal)", _aiProvider.ReceivedPrompt);
        Assert.Contains("What is total revenue?", _aiProvider.ReceivedPrompt);
    }

    [Fact]
    public async Task GenerateSqlAsync_PassesCancellationTokenToSchemaAndAIProviders()
    {
        using var cts = new CancellationTokenSource();

        await CreateService().GenerateSqlAsync("Any question", cts.Token);

        Assert.Equal(cts.Token, _schemaProvider.ReceivedToken);
        Assert.Equal(cts.Token, _aiProvider.ReceivedToken);
    }

    [Fact]
    public async Task GenerateSqlAsync_AIProviderFails_RethrowsAndDoesNotExecute()
    {
        _aiProvider.ExceptionToThrow = new InvalidOperationException("provider down");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().GenerateSqlAsync("Any question"));

        Assert.Equal("provider down", ex.Message);
        Assert.Empty(_executor.ExecutedSql);
    }

    [Fact]
    public async Task GenerateSqlAsync_ExecutorFails_Rethrows()
    {
        _executor.ExceptionToThrow = new TimeoutException("query timed out");

        await Assert.ThrowsAsync<TimeoutException>(
            () => CreateService().GenerateSqlAsync("Any question"));
    }
}
