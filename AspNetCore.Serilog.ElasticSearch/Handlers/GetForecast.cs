using AspNetCore.Serilog.ElasticSearch.Infrastructure.Logging;
using MediatR;

namespace AspNetCore.Serilog.ElasticSearch.Handlers;

public static partial class GetForecast
{
    public sealed record Query : IRequest<Dto>;

    public sealed record Dto(DateOnly Date, int TemperatureC, [SensitiveData] string? Summary)
    {
        [SensitiveData]
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    internal sealed class Handler(IMediator mediator, ILogger<Handler> logger)
        : IRequestHandler<Query, Dto>
    {
        public async Task<Dto> Handle(Query request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Inner Handler log - Start");
            var willThrow = Random.Shared.Next(100) % 2 == 0;
            if (willThrow)
            {
                throw new Exception("Uh oh");
            }

            var innerResponse = await mediator.Send(new GetForecastInner.Query(), cancellationToken);

            logger.LogInformation("Inner Handler log - End");

            var response = innerResponse.Select(r => new Dto(r.Date, r.TemperatureC, r.Summary)).First();

            return response;
        }
    }
}