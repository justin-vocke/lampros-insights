using System.Diagnostics;
using LamprosInsights.Application.Features.Analytics.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LamprosInsights.Api.ErrorHandling;

/// <summary>
/// Converts every unhandled exception into a generic RFC 9457 problem response.
/// Exception details are logged server-side only and never returned to the client.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>Non-standard status (nginx convention) for requests aborted by the client.</summary>
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException
            && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation(
                "Request cancelled by client. Method: {HttpMethod}, Path: {RequestPath}",
                httpContext.Request.Method,
                httpContext.Request.Path.Value);

            httpContext.Response.StatusCode = ClientClosedRequest;
            return true;
        }

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        if (exception is QueryTimeoutException timeout)
        {
            logger.LogWarning(
                "Query execution timed out. TimeoutSeconds: {TimeoutSeconds}, Method: {HttpMethod}, Path: {RequestPath}, TraceId: {TraceId}",
                timeout.TimeoutSeconds,
                httpContext.Request.Method,
                httpContext.Request.Path.Value,
                traceId);

            httpContext.Response.StatusCode = StatusCodes.Status504GatewayTimeout;

            var timeoutProblem = new ProblemDetails
            {
                Status = StatusCodes.Status504GatewayTimeout,
                Title = "The query took too long to run.",
                Detail = $"It was stopped after {timeout.TimeoutSeconds} seconds. Try a more specific question."
            };
            timeoutProblem.Extensions["traceId"] = traceId;

            await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = timeoutProblem
            });

            return true;
        }

        logger.LogError(
            exception,
            "Unhandled exception. Method: {HttpMethod}, Path: {RequestPath}, TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path.Value,
            traceId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = "Quote the trace ID when reporting this issue."
        };
        problemDetails.Extensions["traceId"] = traceId;

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });

        return true;
    }
}
