using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AspNetCore.Serilog.ElasticSearch.Infrastructure.Persistence;

public static class StartupExtensions
{
    public static IHostApplicationBuilder ConfigureDatabase(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<WeatherContext>(
            options =>
            {
                options.EnableDetailedErrors();
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.ConfigureWarnings(
                        warningsConfiguration =>
                        {
                            warningsConfiguration.Log(
                                CoreEventId.FirstWithoutOrderByAndFilterWarning,
                                CoreEventId.RowLimitingOperationWithoutOrderByWarning,
                                CoreEventId.StartedTracking,
                                CoreEventId.SaveChangesStarting);
                        });
                }

                var connectionString = builder.Configuration.GetConnectionString("MySql");
                    
                options.UseMySql(
                    connectionString,
                    ServerVersion.Parse("8.0.23-mysql"));
            });
        
        return builder;
    }
}