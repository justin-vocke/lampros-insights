using LamprosInsights.Application.Features.Analytics.Abstractions;
using LamprosInsights.Application.Features.Analytics.Services;
using LamprosInsights.Infrastructure.Persistence;
using LamprosInsights.Infrastructure.Persistence.Schema;
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
    IAnalyticsService,
    AnalyticsService>();
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
