using System.Reflection;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
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
    // public static IHostBuilder ConfigureSerilog(this IHostBuilder host)
    // {
    //     return host.UseSerilog(
    //         (hostContext, loggerConfiguration) =>
    //         {
    //             loggerConfiguration.MinimumLevel.Information();
    //             loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
    //             loggerConfiguration.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);
    //
    //             loggerConfiguration.Enrich.FromLogContext();
    //             loggerConfiguration.Enrich.WithMachineName();
    //             loggerConfiguration.Enrich.WithEnvironmentName();
    //             loggerConfiguration.Enrich.WithExceptionDetails(
    //                 new DestructuringOptionsBuilder().WithDestructurers(
    //                     new[]
    //                     {
    //                         new ApiExceptionDestructurer()
    //                     }));
    //
    //             loggerConfiguration.WriteTo.Console();
    //             loggerConfiguration.WriteTo.Elasticsearch(
    //                 new ElasticsearchSinkOptions(new Uri(hostContext.Configuration["ElasticSearch:Uri"]!))
    //                 {
    //                     AutoRegisterTemplate = true,
    //                     IndexFormat =
    //                         $"{Assembly.GetExecutingAssembly().GetName().Name!.ToLowerInvariant()}-{DateTimeOffset.Now:yyyy-MM}"
    //                 });
    //         });
    // }

    public static WebApplicationBuilder EnableLoggingRedaction(this WebApplicationBuilder builder)
    {
        var loggerConfiguration = new LoggerConfiguration();
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
            new ElasticsearchSinkOptions(new Uri(builder.Configuration["ElasticSearch:Uri"]!))
            {
                AutoRegisterTemplate = true,
                IndexFormat =
                    $"{Assembly.GetExecutingAssembly().GetName().Name!.ToLowerInvariant()}-{DateTimeOffset.Now:yyyy-MM}"
            });

        Log.Logger = loggerConfiguration.CreateLogger();
        
        builder.Services.AddRedaction(
            options =>
            {
                options.SetRedactor<ErasingRedactor>(new DataClassificationSet(LoggingTaxonomy.SensitiveData));
            });

        builder.Services.AddLogging(
            options =>
            {
                options.EnableRedaction();
                options.AddSerilog(Log.Logger, true);
            });

        return builder;
    }
}