using System.Data;
using System.Text;
using LamprosInsights.Application.Features.Analytics.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace LamprosInsights.Infrastructure.Persistence.Schema;

public class SqlServerSchemaProvider : ISchemaProvider
{
    private readonly string _connectionString;

    public SqlServerSchemaProvider(
        IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Database connection string not found.");
    }

    public async Task<string> GetSchemaContextAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                TABLE_NAME,
                COLUMN_NAME,
                DATA_TYPE
            FROM INFORMATION_SCHEMA.COLUMNS
            ORDER BY
                TABLE_NAME,
                ORDINAL_POSITION
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var schema = new Dictionary<string, List<string>>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var tableName = reader.GetString(0);
            var columnName = reader.GetString(1);
            var dataType = reader.GetString(2);

            if (!schema.ContainsKey(tableName))
            {
                schema[tableName] = [];
            }

            schema[tableName]
                .Add($"- {columnName} ({dataType})");
        }

        var sb = new StringBuilder();

        foreach (var table in schema)
        {
            sb.AppendLine($"Table: {table.Key}");

            foreach (var column in table.Value)
            {
                sb.AppendLine(column);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}