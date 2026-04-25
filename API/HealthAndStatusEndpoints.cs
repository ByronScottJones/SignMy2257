using SignMy2257.Models;
using SignMy2257.Services;

namespace SignMy2257.API;

public static class HealthAndStatusEndpoints
{
    public static void MapHealthAndStatusEndpoints(this WebApplication app)
    {
        app.MapGet("/health", GetHealth)
            .WithName("GetHealth")
            .WithDescription("Health check endpoint")
            .AllowAnonymous();

        app.MapGet("/api/status", GetStatus)
            .WithName("GetStatus")
            .WithDescription("Get application status and statistics")
            .AllowAnonymous();
    }

    private static IResult GetHealth(ILogger<Program> logger)
    {
        logger.LogInformation("Health check requested");
        return Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    private static IResult GetStatus(IStatisticsService statisticsService, ILogger<Program> logger)
    {
        try
        {
            var stats = statisticsService.GetStatistics();
            return Results.Ok(new
            {
                status = "operational",
                timestamp = DateTime.UtcNow,
                statistics = stats
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving status");
            return Results.StatusCode(500);
        }
    }
}
