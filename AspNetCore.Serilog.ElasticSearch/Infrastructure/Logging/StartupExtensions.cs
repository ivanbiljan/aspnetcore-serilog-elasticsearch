using System.Reflection;
using Destructurama;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;
using Serilog.Sinks.Elasticsearch;

namespace AspNetCore.Serilog.ElasticSearch.Infrastructure.Logging;

public static class StartupExtensions
{
    public static IHostBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        return builder.Host.UseSerilog(
            (hostContext, loggerConfiguration) =>
            {
                loggerConfiguration.MinimumLevel.Information();
                loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
                loggerConfiguration.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);
                loggerConfiguration.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information);
    
                loggerConfiguration.Enrich.FromLogContext();
                loggerConfiguration.Enrich.WithMachineName();
                loggerConfiguration.Enrich.WithEnvironmentName();
                loggerConfiguration.Enrich.WithExceptionDetails(
                    new DestructuringOptionsBuilder().WithDestructurers(
                        new[]
                        {
                            new ApiExceptionDestructurer()
                        }));

                loggerConfiguration.Destructure.UsingAttributes();
    
                loggerConfiguration.WriteTo.Console();
                loggerConfiguration.WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(new Uri(hostContext.Configuration["ElasticSearch:Uri"]!))
                    {
                        AutoRegisterTemplate = true,
                        IndexFormat =
                            $"{Assembly.GetExecutingAssembly().GetName().Name!.ToLowerInvariant()}-{DateTimeOffset.Now:yyyy-MM}"
                    });
            });
    }

    public static IApplicationBuilder UseSerilogHttpLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(
            options =>
            {
                options.EnrichDiagnosticContext = static (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("User", httpContext.User.Identity?.Name);
                    diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
                };
            });
    }
}