using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Serilog.ElasticSearch.Infrastructure.Persistence;

public sealed class Forecast
{
    public required int Id { get; init; }
    
    public required DateTime Date { get; init; }
    
    public required int TemperatureC { get; init; }
    
    public required string Summary { get; init; }
}

public class WeatherContext(DbContextOptions<WeatherContext> options) : DbContext(options)
{
    public DbSet<Forecast> Forecasts => Set<Forecast>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Forecast>().HasData(
            new Forecast
            {
                Id = 1,
                Date = DateTime.UtcNow.AddDays(-3),
                TemperatureC = 23,
                Summary = "Warm"
            },
            new Forecast
            {
                Id = 2,
                Date = DateTime.UtcNow.AddDays(-2),
                TemperatureC = 11,
                Summary = "Chilly"
            },
            new Forecast
            {
                Id = 3,
                Date = DateTime.UtcNow.AddDays(-1),
                TemperatureC = 18,
                Summary = "Mild"
            },
            new Forecast
            {
                Id = 4,
                Date = DateTime.UtcNow,
                TemperatureC = 33,
                Summary = "Hot"
            });
        
        base.OnModelCreating(modelBuilder);
    }
}