namespace LamprosInsights.Application.Features.Analytics.Exceptions
{
    /// <summary>
    /// Thrown when query execution exceeds the configured timeout.
    /// The message is fixed and never contains the SQL.
    /// </summary>
    public sealed class QueryTimeoutException : Exception
    {
        public int TimeoutSeconds { get; }

        public QueryTimeoutException(int timeoutSeconds, Exception? innerException = null)
            : base("The query exceeded the configured execution timeout.", innerException)
        {
            TimeoutSeconds = timeoutSeconds;
        }
    }
}
