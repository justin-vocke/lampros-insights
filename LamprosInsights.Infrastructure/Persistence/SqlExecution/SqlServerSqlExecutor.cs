using LamprosInsights.Application.Features.Analytics.Abstractions;
using LamprosInsights.Application.Features.Analytics.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Infrastructure.Persistence.SqlExecution
{
    public class SqlServerSqlExecutor : ISqlExecutor
    {
        private readonly string _connectionString;

        public SqlServerSqlExecutor(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string not configured.");
        }
        public async Task<AnalyticsQueryResult> ExecuteAsync(string sql, CancellationToken cancellationToken = default)
        {
            var columnNames = new List<string>();
            var columns = new List<ColumnDefinition>();
            var result = new AnalyticsQueryResult();

            var stopwatch = Stopwatch.StartNew();

            using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync(cancellationToken);

            using var command =
                new SqlCommand(sql, connection);

            using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                columnNames.Add(columnName);

                columns.Add(new ColumnDefinition 
                { 
                    Name = columnName,
                    Type = reader.GetFieldType(i).Name
                });
            }
            result.Columns = columns;

            while(await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>();

                for(int i = 0; i < columnNames.Count; i++)
                {
                    var value = reader.GetValue(i);

                    row[columnNames[i]] = 
                        value == DBNull.Value
                        ? null
                        : value;
                }
                result.Rows.Add(row);
            }
            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.RowCount = result.Rows.Count;

            return result;
            
        }
    }
}
