using AspNetCore.Serilog.ElasticSearch.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Serilog.ElasticSearch.Handlers;

public static class GetForecastInner
{
    public sealed record Query : IRequest<IEnumerable<Dto>>;

    public sealed record Dto(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    internal sealed class Handler(WeatherContext dbContext) : IRequestHandler<Query, IEnumerable<Dto>>
    {
        public async Task<IEnumerable<Dto>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await dbContext.Forecasts
                .Select(f => new Dto(DateOnly.FromDateTime(f.Date), f.TemperatureC, f.Summary))
                .ToListAsync(cancellationToken);
        }
    }
}