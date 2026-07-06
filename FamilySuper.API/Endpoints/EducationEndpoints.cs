using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FamilySuper.API.Endpoints;

public static class EducationEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/education").WithTags("Education");

        group.MapGet("/records", async (IEducationService svc, long? memberId, string? subject, CancellationToken ct) =>
            Results.Ok(await svc.GetRecordsAsync(memberId, subject, ct)));

        group.MapPost("/records", async (IEducationService svc, EducationQaRequest request, CancellationToken ct) =>
        {
            var record = await svc.AddQaAsync(request.Question, request.Answer, request.Subject, request.MemberId, ct);
            return Results.Created($"/api/education/records/{record.Id}", record);
        });

        group.MapDelete("/records/{id:long}", async (long id, IEducationService svc, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }
}

public record EducationQaRequest(string Question, string Answer, string? Subject, long? MemberId);
