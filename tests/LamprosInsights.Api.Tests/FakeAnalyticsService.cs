using LamprosInsights.Application.Features.Analytics.Abstractions;
using LamprosInsights.Application.Features.Analytics.Dtos;

namespace LamprosInsights.Api.Tests;

public sealed class FakeAnalyticsService : IAnalyticsService
{
    public Exception? ExceptionToThrow { get; set; }

    public GenerateSqlResponse ResponseToReturn { get; set; } = new();

    public Task<GenerateSqlResponse> GenerateSqlAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return Task.FromResult(ResponseToReturn);
    }
}
