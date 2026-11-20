using System.Data.Common;
using LamprosInsights.Application.Features.Analytics.Dtos;

namespace LamprosInsights.Infrastructure.Persistence.SqlExecution
{
    internal static class QueryResultReader
    {
        /// <summary>
        /// Reads column definitions and at most <paramref name="maxRows"/> rows.
        /// One extra read detects truncation; that extra row is discarded.
        /// </summary>
        internal static async Task<(List<ColumnDefinition> Columns, List<Dictionary<string, object?>> Rows, bool IsTruncated)>
            ReadAsync(DbDataReader reader, int maxRows, CancellationToken cancellationToken)
        {
            var columnNames = new List<string>(reader.FieldCount);
            var columns = new List<ColumnDefinition>(reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                columnNames.Add(name);
                columns.Add(new ColumnDefinition
                {
                    Name = name,
                    Type = reader.GetFieldType(i).Name
                });
            }

            var rows = new List<Dictionary<string, object?>>();

            while (rows.Count < maxRows && await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < columnNames.Count; i++)
                {
                    var value = reader.GetValue(i);
                    row[columnNames[i]] = value == DBNull.Value ? null : value;
                }
                rows.Add(row);
            }

            var isTruncated = rows.Count >= maxRows && await reader.ReadAsync(cancellationToken);

            return (columns, rows, isTruncated);
        }
    }
}
