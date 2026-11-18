using LamprosInsights.Application.Features.Analytics.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LamprosInsights.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public FakeAnalyticsService Analytics { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development matches the environment where the developer exception page
        // would otherwise take over, so the tests prove problem+json wins there too.
        builder.UseEnvironment("Development");

        // Harmless dummy values; nothing in these tests touches a database or OpenAI.
        builder.ConfigureAppConfiguration((_, config) =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=Dummy;Trusted_Connection=True;",
                ["OpenAI:ApiKey"] = "dummy-key"
            }));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAnalyticsService>();
            services.AddSingleton<IAnalyticsService>(Analytics);
        });
    }
}
