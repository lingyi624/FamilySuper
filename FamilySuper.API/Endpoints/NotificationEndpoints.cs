using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FamilySuper.API.Endpoints;

public static class NotificationEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications");

        group.MapGet("/", async (INotificationService svc, int? limit, CancellationToken ct) =>
            Results.Ok(await svc.GetPendingAsync(limit ?? 50, ct)));

        group.MapPost("/{id:long}/read", async (long id, INotificationService svc, CancellationToken ct) =>
        {
            await svc.MarkReadAsync(id, ct);
            return Results.Ok(new { read = true });
        });

        group.MapPost("/read-all", async (INotificationService svc, CancellationToken ct) =>
        {
            await svc.MarkAllReadAsync(ct);
            return Results.Ok(new { readAll = true });
        });
    }
}
