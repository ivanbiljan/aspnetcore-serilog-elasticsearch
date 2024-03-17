using System.Diagnostics;
using MediatR;

namespace AspNetCore.Serilog.ElasticSearch.Infrastructure.Behaviors;

internal sealed partial class LoggingBehavior<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    ILogger<LoggingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var additionalLogProperties = new Dictionary<string, object?>
        {
            ["Request"] = request
        };

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            additionalLogProperties["User"] = httpContext.User.Identity?.Name;
            additionalLogProperties["RemoteIP"] = httpContext.Connection.RemoteIpAddress;
        }

        try
        {
            var response = await next();
            stopwatch.Stop();

            using (logger.BeginScope(additionalLogProperties))
            {
                LogSuccess(logger, typeof(TRequest).FullName!, typeof(TResponse).Name, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            using (logger.BeginScope(additionalLogProperties))
            {
                LogFailure(logger, typeof(TRequest).FullName!, typeof(TResponse).Name, stopwatch.ElapsedMilliseconds, ex);
            }

            throw;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "IRequest<{RequestType}, {ResponseType}> returned an exception after {ElapsedTime} ms: {Exception}")]
    public static partial void LogFailure(
        ILogger logger,
        string requestType,
        string responseType,
        long elapsedTime,
        Exception exception);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "IRequest<{RequestType}, {ResponseType}> executed in {ElapsedTime} ms")]
    public static partial void LogSuccess(ILogger logger, string requestType, string responseType, long elapsedTime);
}