using MarketingSite.Services;
using Microsoft.Extensions.Logging;

namespace MarketingSite.RouteHandler;

public record HealthCheckResponse(string Status, string? Error = null);

public class HealthCheckRouteHandler(ICacheService cacheService, ILogger<HealthCheckRouteHandler> logger)
{
    public async Task<IResult> HandleRequest(HttpContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Verify Redis is reachable by performing a lightweight round-trip
            await cacheService.PingAsync(cancellationToken);
            return Results.Ok(new HealthCheckResponse("healthy"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed: Redis connectivity check failed.");
            return Results.Ok(new HealthCheckResponse("unhealthy", "Cache connectivity check failed."));
        }
    }
}