using System.Reflection;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (hostContext, loggerConfiguration) =>
    {
        loggerConfiguration.MinimumLevel.Information();
        loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
        loggerConfiguration.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);

        loggerConfiguration.Enrich.FromLogContext();
        loggerConfiguration.Enrich.WithMachineName();
        loggerConfiguration.Enrich.WithEnvironmentName();
        loggerConfiguration.Enrich.WithExceptionDetails(
            new DestructuringOptionsBuilder().WithDestructurers(
                new[]
                {
                    new ApiExceptionDestructurer()
                }));

        loggerConfiguration.WriteTo.Console();
        loggerConfiguration.WriteTo.Elasticsearch(
            new ElasticsearchSinkOptions(new Uri(hostContext.Configuration["ElasticSearch:Uri"]!))
            {
                AutoRegisterTemplate = true,
                IndexFormat =
                    $"{Assembly.GetExecutingAssembly().GetName().Name!.ToLowerInvariant()}-{DateTimeOffset.Now:yyyy-MM}"
            });
    });

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(
    options =>
    {
        options.EnrichDiagnosticContext = static (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("User", httpContext.User.Identity?.Name);
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
        };
    });

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet(
        "/weatherforecast",
        () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(
                    index =>
                        new WeatherForecast(
                            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                            Random.Shared.Next(-20, 55),
                            summaries[Random.Shared.Next(summaries.Length)]
                        ))
                .ToArray();

            return forecast;
        })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}