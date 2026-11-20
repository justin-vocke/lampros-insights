using System.Data;
using LamprosInsights.Infrastructure.Persistence.SqlExecution;

namespace LamprosInsights.Infrastructure.Tests;

public class QueryResultReaderTests
{
    private static DataTable CreateTable(int rowCount)
    {
        var table = new DataTable();
        table.Columns.Add("Id", typeof(int));
        table.Columns.Add("Name", typeof(string));
        for (var i = 1; i <= rowCount; i++)
            table.Rows.Add(i, $"Name{i}");
        return table;
    }

    private static Task<(List<LamprosInsights.Application.Features.Analytics.Dtos.ColumnDefinition> Columns, List<Dictionary<string, object?>> Rows, bool IsTruncated)>
        Read(DataTable table, int maxRows) =>
        QueryResultReader.ReadAsync(table.CreateDataReader(), maxRows, CancellationToken.None);

    [Fact]
    public async Task ZeroRows_ReturnsColumnsAndNoRows_NotTruncated()
    {
        var (columns, rows, truncated) = await Read(CreateTable(0), 5);

        Assert.Equal(2, columns.Count);
        Assert.Empty(rows);
        Assert.False(truncated);
    }

    [Fact]
    public async Task FewerThanMax_ReturnsAllRows_NotTruncated()
    {
        var (_, rows, truncated) = await Read(CreateTable(3), 5);

        Assert.Equal(3, rows.Count);
        Assert.False(truncated);
    }

    [Fact]
    public async Task ExactlyMax_ReturnsAllRows_NotTruncated()
    {
        var (_, rows, truncated) = await Read(CreateTable(5), 5);

        Assert.Equal(5, rows.Count);
        Assert.False(truncated);
    }

    [Fact]
    public async Task MaxPlusOne_ReturnsMaxRows_Truncated()
    {
        var (_, rows, truncated) = await Read(CreateTable(6), 5);

        Assert.Equal(5, rows.Count);
        Assert.Equal(5, rows[^1]["Id"]);
        Assert.True(truncated);
    }

    [Fact]
    public async Task ManyMoreThanMax_ReturnsMaxRows_Truncated()
    {
        var (_, rows, truncated) = await Read(CreateTable(500), 5);

        Assert.Equal(5, rows.Count);
        Assert.True(truncated);
    }

    [Fact]
    public async Task DbNull_IsMappedToNull()
    {
        var table = new DataTable();
        table.Columns.Add("Name", typeof(string));
        table.Rows.Add(DBNull.Value);

        var (_, rows, _) = await Read(table, 5);

        Assert.Null(rows[0]["Name"]);
    }

    [Fact]
    public async Task ColumnNamesAndTypes_ArePreservedInOrder()
    {
        var (columns, _, _) = await Read(CreateTable(1), 5);

        Assert.Equal(["Id", "Name"], columns.Select(c => c.Name));
        Assert.Equal(["Int32", "String"], columns.Select(c => c.Type));
    }
}
