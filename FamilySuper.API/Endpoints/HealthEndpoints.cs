using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FamilySuper.API.Endpoints;

public static class HealthEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/health", () => new
        {
            Status = "Healthy",
            Timestamp = DateTimeOffset.UtcNow,
            Version = "1.0.0"
        });

        app.MapGet("/health/ready", () => Results.Ok(new { Ready = true }));
    }
}
