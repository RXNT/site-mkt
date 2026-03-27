using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace MarketingApi.RouteHandler;

public record HealthCheckResponse(string Status, string? Error = null);

public class HealthCheckRouteHandler(IConfiguration configuration, ILogger<HealthCheckRouteHandler> logger)
{
    public async Task<IResult> HandleRequest(HttpContext context, CancellationToken cancellationToken)
    {
        try
        {
            await TestSqlConnection(cancellationToken);
            return Results.Ok(new HealthCheckResponse("healthy"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed: SQL connection could not be established.");
            return Results.Ok(new HealthCheckResponse("unhealthy", "Database connectivity check failed."));
        }
    }

    private async Task TestSqlConnection(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(configuration["DB_CONNECTION_STRING"]);
        await connection.OpenAsync(cancellationToken);
        // Perform a simple query to test the connection
        var cmd = new SqlCommand("SELECT 1", connection);
        await cmd.ExecuteScalarAsync(cancellationToken);
        await connection.CloseAsync();
    }
}