using LamprosInsights.Application.Features.Analytics.Prompts;

namespace LamprosInsights.Application.Tests.Features.Analytics;

public class AnalyticsPromptBuilderTests
{
    private readonly AnalyticsPromptBuilder _builder = new();

    [Fact]
    public void BuildSqlGenerationPrompt_IncludesSchemaAndQuestion()
    {
        var prompt = _builder.BuildSqlGenerationPrompt(
            "Table: Products (Id int, Price decimal)",
            "Which products cost more than 50?");

        Assert.Contains("Table: Products (Id int, Price decimal)", prompt);
        Assert.Contains("Which products cost more than 50?", prompt);
    }

    [Fact]
    public void BuildSqlGenerationPrompt_RestrictsOutputToReadOnlySqlServerSelects()
    {
        var prompt = _builder.BuildSqlGenerationPrompt("schema", "question");

        Assert.Contains("SQL Server", prompt);
        Assert.Contains("ONLY generate SELECT statements", prompt);
        Assert.Contains("NEVER generate UPDATE, DELETE, DROP, INSERT, or ALTER", prompt);
    }
}
