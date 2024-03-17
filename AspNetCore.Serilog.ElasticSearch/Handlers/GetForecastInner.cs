using MediatR;

namespace AspNetCore.Serilog.ElasticSearch.Handlers;

public static class GetForecastInner
{
    public sealed record Query : IRequest<IEnumerable<Dto>>;

    public sealed record Dto(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    internal sealed class Handler : IRequestHandler<Query, IEnumerable<Dto>>
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        public async Task<IEnumerable<Dto>> Handle(Query request, CancellationToken cancellationToken)
        {
            return Enumerable.Range(1, 5).Select(
                    index =>
                        new Dto(
                            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                            Random.Shared.Next(-20, 55),
                            Summaries[Random.Shared.Next(Summaries.Length)]
                        ))
                .ToArray();
        }
    }
}