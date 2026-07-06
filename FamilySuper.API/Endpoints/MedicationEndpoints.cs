using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FamilySuper.API.Endpoints;

public static class MedicationEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/medications").WithTags("Medications");

        group.MapGet("/plans", async (IMedicationService svc, long? memberId, bool? activeOnly, CancellationToken ct) =>
            Results.Ok(await svc.GetPlansAsync(memberId, activeOnly ?? true, ct)));

        group.MapGet("/plans/{id:long}", async (long id, IMedicationService svc, CancellationToken ct) =>
        {
            var plan = await svc.GetPlanByIdAsync(id, ct);
            return plan is null ? Results.NotFound() : Results.Ok(plan);
        });

        group.MapPost("/plans", async (MedicationPlan plan, IMedicationService svc, CancellationToken ct) =>
        {
            var created = await svc.AddPlanAsync(plan, ct);
            return Results.Created($"/api/medications/plans/{created.Id}", created);
        });

        group.MapPut("/plans/{id:long}", async (long id, MedicationPlan input, IMedicationService svc, CancellationToken ct) =>
        {
            var existing = await svc.GetPlanByIdAsync(id, ct);
            if (existing is null) return Results.NotFound();

            existing.MemberId = input.MemberId;
            existing.DrugName = input.DrugName;
            existing.Dosage = input.Dosage;
            existing.Frequency = input.Frequency;
            existing.TimesPerDay = input.TimesPerDay;
            existing.StartDate = input.StartDate;
            existing.EndDate = input.EndDate;
            existing.Notes = input.Notes;
            existing.IsActive = input.IsActive;
            await svc.UpdatePlanAsync(existing, ct);
            return Results.Ok(existing);
        });

        group.MapDelete("/plans/{id:long}", async (long id, IMedicationService svc, CancellationToken ct) =>
        {
            await svc.DeletePlanAsync(id, ct);
            return Results.NoContent();
        });

        group.MapGet("/plans/{id:long}/records", async (long id, IMedicationService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetRecordsAsync(id, ct)));

        group.MapPost("/plans/{id:long}/records", async (long id, MedicationRecord input, IMedicationService svc, CancellationToken ct) =>
        {
            input.MedicationPlanId = id;
            var created = await svc.RecordDoseAsync(input, ct);
            return Results.Created($"/api/medications/plans/{id}/records/{created.Id}", created);
        });

        group.MapGet("/reminders", async (IMedicationService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetDueRemindersAsync(ct)));
    }
}
