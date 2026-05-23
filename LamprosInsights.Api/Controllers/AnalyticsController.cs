using LamprosInsights.Application.Features.Analytics.Abstractions;
using LamprosInsights.Application.Features.Analytics.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace LamprosInsights.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController(
    IAnalyticsService analyticsService) : ControllerBase
{
    private readonly IAnalyticsService _analyticsService = analyticsService;

    //[HttpGet("schema")]
    //public async Task<IActionResult> GetSchema(
    //    CancellationToken cancellationToken)
    //{
    //    var schema = await _analyticsService
    //        .GetSchemaAsync(cancellationToken);

    //    return Ok(schema);
    //}

    [HttpPost("generate-sql")]
    public async Task<IActionResult> GenerateSql(
    GenerateSqlRequest request,
    CancellationToken cancellationToken)
    {
        var sql = await _analyticsService.GenerateSqlAsync(
            request.Question,
            cancellationToken);

        return Ok(new GenerateSqlResponse
        {
            Sql = sql
        });
    }
}