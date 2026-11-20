using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LamprosInsights.Application.Features.Analytics.Dtos;
using LamprosInsights.Application.Features.Analytics.Exceptions;

namespace LamprosInsights.Api.Tests;

public class ApiErrorHandlingTests : IClassFixture<ApiFactory>
{
    private const string ProblemJson = "application/problem+json";
    private const string GenerateSqlUrl = "/api/analytics/generate-sql";

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public ApiErrorHandlingTests(ApiFactory factory)
    {
        _factory = factory;
        _factory.Analytics.ExceptionToThrow = null;
        _client = factory.CreateClient(new() { AllowAutoRedirect = false });
    }

    private static StringContent Json(string body) =>
        new(body, Encoding.UTF8, "application/json");

    [Fact]
    public async Task UnhandledException_Returns500ProblemDetails_WithoutLeakingDetails()
    {
        _factory.Analytics.ExceptionToThrow =
            new InvalidOperationException("Password=secret;Server=prod-sql");

        var response = await _client.PostAsync(GenerateSqlUrl, Json("""{"question":"q"}"""));
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);

        using var doc = JsonDocument.Parse(raw);
        Assert.True(doc.RootElement.TryGetProperty("title", out _));
        Assert.Equal(500, doc.RootElement.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(doc.RootElement.GetProperty("traceId").GetString()));

        Assert.DoesNotContain("secret", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("prod-sql", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(nameof(InvalidOperationException), raw);
        Assert.DoesNotContain(" at ", raw);
    }

    [Fact]
    public async Task QueryTimeout_Returns504ProblemDetails_WithoutLeakingDetails()
    {
        _factory.Analytics.ExceptionToThrow =
            new QueryTimeoutException(30, new Exception("Server=prod-sql;Password=secret"));

        var response = await _client.PostAsync(GenerateSqlUrl, Json("""{"question":"q"}"""));
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);

        using var doc = JsonDocument.Parse(raw);
        Assert.Equal("The query took too long to run.", doc.RootElement.GetProperty("title").GetString());
        Assert.False(string.IsNullOrWhiteSpace(doc.RootElement.GetProperty("traceId").GetString()));

        Assert.DoesNotContain("secret", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("prod-sql", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(nameof(QueryTimeoutException), raw);
        Assert.DoesNotContain(" at ", raw);
    }

    [Fact]
    public async Task UnknownRoute_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync("/api/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("")]
    public async Task MalformedOrMissingBody_Returns400ProblemDetails_WithTraceId(string body)
    {
        var response = await _client.PostAsync(GenerateSqlUrl, Json(body));
        var raw = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ProblemJson, response.Content.Headers.ContentType?.MediaType);

        using var doc = JsonDocument.Parse(raw);
        Assert.False(string.IsNullOrWhiteSpace(doc.RootElement.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task InvalidSql_Returns200WithValidationErrors_AndNoErrorMessageProperty()
    {
        _factory.Analytics.ResponseToReturn = new GenerateSqlResponse
        {
            Sql = "DROP TABLE x",
            IsValid = false,
            ValidationErrors = ["Only SELECT statements are allowed."]
        };

        var response = await _client.PostAsync(GenerateSqlUrl, Json("""{"question":"q"}"""));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(doc.RootElement.GetProperty("isValid").GetBoolean());
        Assert.Equal(1, doc.RootElement.GetProperty("validationErrors").GetArrayLength());
        Assert.False(doc.RootElement.TryGetProperty("errorMessage", out _));
    }
}
