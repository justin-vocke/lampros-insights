using LamprosInsights.Application.Features.Analytics.Abstractions;
using LamprosInsights.Application.Features.Analytics.Prompts;
using LamprosInsights.Application.Features.Analytics.Services;
using LamprosInsights.Application.Features.Analytics.Validation;
using LamprosInsights.Infrastructure.AI.OpenAI;
using LamprosInsights.Infrastructure.Persistence;
using LamprosInsights.Infrastructure.Persistence.Schema;
using LamprosInsights.Infrastructure.Persistence.SqlExecution;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AnalyticsDbContext>(
    options =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString(
                "DefaultConnection"));
    });
builder.Services.AddScoped<
    ISchemaProvider,
    SqlServerSchemaProvider>();

builder.Services.AddScoped<
    ISqlValidator,
    SqlValidator>();

builder.Services.AddScoped<
    ISqlExecutor,
    SqlServerSqlExecutor>();

builder.Services.AddScoped<AnalyticsPromptBuilder>();

builder.Services.AddScoped<
    IAnalyticsService,
    AnalyticsService>();

builder.Services.AddScoped<
    IAIProvider,
    OpenAIProvider>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
