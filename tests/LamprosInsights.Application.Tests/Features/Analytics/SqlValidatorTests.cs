using LamprosInsights.Application.Features.Analytics.Validation;

namespace LamprosInsights.Application.Tests.Features.Analytics;

public class SqlValidatorTests
{
    private readonly SqlValidator _validator = new();

    [Theory]
    [InlineData("SELECT TOP 100 Name FROM Customers")]
    [InlineData("select Name from Customers")]
    [InlineData("   SELECT Name FROM Customers   ")]
    [InlineData("SELECT c.Name, COUNT(o.Id) AS OrderCount FROM Customers c JOIN Orders o ON o.CustomerId = c.Id GROUP BY c.Name")]
    public void Validate_ReadOnlySelect_IsValid(string sql)
    {
        var result = _validator.Validate(sql);

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptySql_IsInvalid(string? sql)
    {
        var result = _validator.Validate(sql!);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Theory]
    [InlineData("WITH x AS (SELECT 1 AS n) SELECT n FROM x")]
    [InlineData("SHOW TABLES")]
    public void Validate_NotStartingWithSelect_IsInvalid(string sql)
    {
        var result = _validator.Validate(sql);

        Assert.Contains("SQL must begin with SELECT statement", result.Errors);
    }

    [Theory]
    [InlineData("DROP TABLE Customers", "DROP")]
    [InlineData("DELETE FROM Customers", "DELETE")]
    [InlineData("UPDATE Customers SET Name = 'x'", "UPDATE")]
    [InlineData("INSERT INTO Customers (Name) VALUES ('x')", "INSERT")]
    [InlineData("ALTER TABLE Customers ADD Foo int", "ALTER")]
    [InlineData("TRUNCATE TABLE Customers", "TRUNCATE")]
    [InlineData("EXEC sp_who", "EXEC")]
    public void Validate_WriteOrExecStatement_IsInvalid(string sql, string keyword)
    {
        var result = _validator.Validate(sql);

        Assert.False(result.IsValid);
        Assert.Contains($"SQL cannot contain {keyword}", result.Errors);
    }

    [Fact]
    public void Validate_SelectFollowedByWriteStatement_IsInvalid()
    {
        var result = _validator.Validate("SELECT 1; DROP TABLE Customers");

        Assert.Contains("SQL cannot contain DROP", result.Errors);
    }

    [Theory]
    [InlineData("SELECT Name FROM Customers -- comment")]
    [InlineData("SELECT Name /* comment */ FROM Customers")]
    public void Validate_Comments_AreInvalid(string sql)
    {
        var result = _validator.Validate(sql);

        Assert.Contains("SQL comments are not allowed", result.Errors);
    }

    // Known limitations of the current keyword-based validator. These assert the
    // intended behavior and are skipped until parser-based validation lands.

    [Fact(Skip = "Known limitation: keyword matching rejects identifiers like UpdatedAt. Fix in LAM-65/LAM-66.")]
    public void Validate_IdentifierContainingKeyword_IsValid()
    {
        var result = _validator.Validate("SELECT UpdatedAt FROM Orders");

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact(Skip = "Known limitation: multiple statements are not rejected. Fix in LAM-65/LAM-66.")]
    public void Validate_MultipleSelectStatements_IsInvalid()
    {
        var result = _validator.Validate("SELECT 1; SELECT 2");

        Assert.False(result.IsValid);
    }
}
