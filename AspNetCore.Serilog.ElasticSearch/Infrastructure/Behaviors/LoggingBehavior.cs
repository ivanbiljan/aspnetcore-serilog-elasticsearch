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

        var requestType = typeof(TRequest);
        var handlerName = requestType.DeclaringType?.FullName ?? requestType.FullName!;

        try
        {
            var response = await next();
            stopwatch.Stop();

            using (logger.BeginScope(additionalLogProperties))
            {
                LogSuccess(logger, handlerName, stopwatch.Elapsed.TotalMilliseconds, response);
            }

            return response;
        }
        catch (Exception ex)
        {
            using (logger.BeginScope(additionalLogProperties))
            {
                LogFailure(logger, handlerName, stopwatch.Elapsed.TotalMilliseconds);
            }

            throw;
        }
    }
    
    [LoggerMessage(
        
        Level = LogLevel.Error,
        Message = "{Handler} returned an exception after {ElapsedTime} ms")]
    public static partial void LogFailure(ILogger logger, string handler, double elapsedTime);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "{Handler} executed in {ElapsedTime} ms")]
    public static partial void LogSuccess(
        ILogger logger,
        string handler,
        double elapsedTime,
        [LogProperties] TResponse response);
}