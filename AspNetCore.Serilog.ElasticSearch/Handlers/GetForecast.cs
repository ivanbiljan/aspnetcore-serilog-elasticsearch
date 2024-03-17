using MediatR;

namespace AspNetCore.Serilog.ElasticSearch.Handlers;

public static class GetForecast
{
    public sealed record Query : IRequest<IEnumerable<Dto>>;

    public sealed record Dto(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    internal sealed class Handler(IMediator mediator) : IRequestHandler<Query, IEnumerable<Dto>>
    {
        public async Task<IEnumerable<Dto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var innerResponse = await mediator.Send(new GetForecastInner.Query(), cancellationToken);

            return innerResponse.Select(r => new Dto(r.Date, r.TemperatureC, r.Summary));
        }
    }
}