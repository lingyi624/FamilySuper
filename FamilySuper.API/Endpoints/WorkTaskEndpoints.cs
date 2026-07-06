using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TaskStatus = FamilySuper.Core.Enums.TaskStatus;

namespace FamilySuper.API.Endpoints;

public static class WorkTaskEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/work").WithTags("Work");

        group.MapGet("/tasks", async (IWorkTaskService svc, TaskStatus? status, TaskPriority? priority, CancellationToken ct) =>
            Results.Ok(await svc.GetTasksAsync(status, priority, ct)));

        group.MapGet("/tasks/{id:long}", async (long id, IWorkTaskService svc, CancellationToken ct) =>
        {
            var task = await svc.GetByIdAsync(id, ct);
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        group.MapPost("/tasks", async (WorkTask task, IWorkTaskService svc, CancellationToken ct) =>
        {
            var created = await svc.AddAsync(task, ct);
            return Results.Created($"/api/work/tasks/{created.Id}", created);
        });

        group.MapPut("/tasks/{id:long}", async (long id, WorkTask input, IWorkTaskService svc, CancellationToken ct) =>
        {
            var existing = await svc.GetByIdAsync(id, ct);
            if (existing is null) return Results.NotFound();

            existing.Title = input.Title;
            existing.Description = input.Description;
            existing.Priority = input.Priority;
            existing.DueDate = input.DueDate;
            existing.Category = input.Category;
            existing.MemberId = input.MemberId;
            await svc.UpdateAsync(existing, ct);
            return Results.Ok(existing);
        });

        group.MapPatch("/tasks/{id:long}/status", async (long id, TaskStatus status, IWorkTaskService svc, CancellationToken ct) =>
        {
            await svc.UpdateStatusAsync(id, status, ct);
            return Results.NoContent();
        });

        group.MapDelete("/tasks/{id:long}", async (long id, IWorkTaskService svc, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }
}
