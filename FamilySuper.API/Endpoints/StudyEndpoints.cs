using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FamilySuper.API.Endpoints;

public static class StudyEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/study").WithTags("Study");

        group.MapGet("/plans", async (IStudyService svc, long? memberId, bool? activeOnly, CancellationToken ct) =>
            Results.Ok(await svc.GetPlansAsync(memberId, activeOnly ?? true, ct)));

        group.MapGet("/plans/{id:long}", async (long id, IStudyService svc, CancellationToken ct) =>
        {
            var plan = await svc.GetPlanByIdAsync(id, ct);
            return plan is null ? Results.NotFound() : Results.Ok(plan);
        });

        group.MapPost("/plans", async (StudyPlan plan, IStudyService svc, CancellationToken ct) =>
        {
            var created = await svc.AddPlanAsync(plan, ct);
            return Results.Created($"/api/study/plans/{created.Id}", created);
        });

        group.MapPut("/plans/{id:long}", async (long id, StudyPlan input, IStudyService svc, CancellationToken ct) =>
        {
            var existing = await svc.GetPlanByIdAsync(id, ct);
            if (existing is null) return Results.NotFound();
            existing.MemberId = input.MemberId;
            existing.Subject = input.Subject;
            existing.Goal = input.Goal;
            existing.TargetHoursPerWeek = input.TargetHoursPerWeek;
            existing.ProgressPercent = input.ProgressPercent;
            existing.StartDate = input.StartDate;
            existing.EndDate = input.EndDate;
            existing.IsActive = input.IsActive;
            existing.Notes = input.Notes;
            await svc.UpdatePlanAsync(existing, ct);
            return Results.Ok(existing);
        });

        group.MapDelete("/plans/{id:long}", async (long id, IStudyService svc, CancellationToken ct) =>
        {
            await svc.DeletePlanAsync(id, ct);
            return Results.NoContent();
        });

        group.MapGet("/logs", async (IStudyService svc, long? planId, DateTime? from, DateTime? to, CancellationToken ct) =>
            Results.Ok(await svc.GetLogsAsync(planId, from, to, ct)));

        group.MapPost("/logs", async (StudyLog log, IStudyService svc, CancellationToken ct) =>
        {
            var created = await svc.LogStudyAsync(log, ct);
            return Results.Created($"/api/study/logs/{created.Id}", created);
        });

        group.MapGet("/report", async (IStudyService svc, long? memberId, DateTime from, DateTime to, CancellationToken ct) =>
            Results.Ok(await svc.GenerateReportAsync(memberId, from, to, ct)));
    }
}
