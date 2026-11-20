using System.Diagnostics;
using LamprosInsights.Application.Features.Analytics.Abstractions;
using LamprosInsights.Application.Features.Analytics.Dtos;
using LamprosInsights.Application.Features.Analytics.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LamprosInsights.Infrastructure.Persistence.SqlExecution
{
    public class SqlServerSqlExecutor : ISqlExecutor
    {
        private const int SqlTimeoutErrorNumber = -2;

        private readonly string _connectionString;
        private readonly QueryExecutionOptions _options;

        public SqlServerSqlExecutor(
            IConfiguration configuration,
            IOptions<QueryExecutionOptions> options)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string not configured.");
            _options = options.Value;
        }

        public async Task<AnalyticsQueryResult> ExecuteAsync(string sql, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            // Only timeouts after the connection is open are query timeouts; connect timeouts are infrastructure failures.
            var connectionOpened = false;

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);
                connectionOpened = true;

                await using var command = new SqlCommand(sql, connection)
                {
                    CommandTimeout = _options.CommandTimeoutSeconds
                };

                var reader = await command.ExecuteReaderAsync(cancellationToken);
                (List<ColumnDefinition> Columns, List<Dictionary<string, object?>> Rows, bool IsTruncated) read;

                try
                {
                    read = await QueryResultReader.ReadAsync(reader, _options.MaxRows, cancellationToken);

                    // Stop the server streaming the remaining rows; disposing a
                    // SqlDataReader would otherwise drain all of them.
                    if (read.IsTruncated)
                    {
                        command.Cancel();
                    }
                }
                finally
                {
                    await reader.DisposeAsync();
                }

                stopwatch.Stop();

                return new AnalyticsQueryResult
                {
                    Columns = read.Columns,
                    Rows = read.Rows,
                    RowCount = read.Rows.Count,
                    IsTruncated = read.IsTruncated,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (SqlException ex) when (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException("The query was cancelled.", ex, cancellationToken);
            }
            catch (SqlException ex) when (connectionOpened && ex.Number == SqlTimeoutErrorNumber)
            {
                throw new QueryTimeoutException(_options.CommandTimeoutSeconds, ex);
            }
        }
    }
}
