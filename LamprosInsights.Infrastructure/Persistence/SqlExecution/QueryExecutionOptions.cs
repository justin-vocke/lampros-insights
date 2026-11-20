namespace LamprosInsights.Infrastructure.Persistence.SqlExecution
{
    public class QueryExecutionOptions
    {
        public const string SectionName = "QueryExecution";

        /// <summary>Maximum number of rows returned to the caller.</summary>
        public int MaxRows { get; set; } = 100;

        /// <summary>SQL command timeout in seconds.</summary>
        public int CommandTimeoutSeconds { get; set; } = 30;
    }
}
