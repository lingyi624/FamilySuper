using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FamilySuper.API.Endpoints;

public static class ConversationEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/conversations").WithTags("Conversation");

        group.MapGet("/sessions", async (IConversationService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetSessionsAsync(ct)));

        group.MapPost("/sessions", async (CreateSessionRequest request, IConversationService svc, CancellationToken ct) =>
        {
            var session = await svc.CreateSessionAsync(request.Title, request.MemberId, ct);
            return Results.Created($"/api/conversations/sessions/{session.Id}", session);
        });

        group.MapDelete("/sessions/{id:long}", async (long id, IConversationService svc, CancellationToken ct) =>
        {
            await svc.DeleteSessionAsync(id, ct);
            return Results.NoContent();
        });

        group.MapGet("/sessions/{id:long}/messages", async (long id, IConversationService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetMessagesAsync(id, ct)));

        group.MapPost("/sessions/{id:long}/messages", async (long id, AddMessageRequest request, IConversationService svc, CancellationToken ct) =>
        {
            var message = await svc.AddMessageAsync(id, request.Role, request.Content, request.TokenCount, request.ModelId, ct);
            return Results.Created($"/api/conversations/sessions/{id}/messages/{message.Id}", message);
        });
    }
}

public record CreateSessionRequest(string? Title, long? MemberId);
public record AddMessageRequest(MessageRole Role, string Content, int? TokenCount, string? ModelId);
