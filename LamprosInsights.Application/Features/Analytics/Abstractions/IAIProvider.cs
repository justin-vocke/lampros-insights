namespace LamprosInsights.Application.Features.Analytics.Abstractions;

public interface IAIProvider
{
    Task<string> GenerateSqlAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}