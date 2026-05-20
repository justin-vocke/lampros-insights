using LamprosInsights.Application.Features.Analytics.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LamprosInsights.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController(
    IAnalyticsService analyticsService) : ControllerBase
{
    private readonly IAnalyticsService _analyticsService = analyticsService;

    [HttpGet("schema")]
    public async Task<IActionResult> GetSchema(
        CancellationToken cancellationToken)
    {
        var schema = await _analyticsService
            .GetSchemaAsync(cancellationToken);

        return Ok(schema);
    }
}