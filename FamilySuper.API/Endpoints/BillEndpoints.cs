using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FamilySuper.API.Endpoints;

public static class BillEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/bills").WithTags("Bills");

        group.MapGet("/", async (IBillService svc, bool? activeOnly, CancellationToken ct) =>
            Results.Ok(await svc.GetBillsAsync(activeOnly, ct)));

        group.MapGet("/due-soon", async (IBillService svc, int days, CancellationToken ct) =>
            Results.Ok(await svc.GetDueSoonAsync(days == 0 ? 7 : days, ct)));

        group.MapGet("/{id:long}", async (long id, IBillService svc, CancellationToken ct) =>
        {
            var bill = await svc.GetBillByIdAsync(id, ct);
            return bill is null ? Results.NotFound() : Results.Ok(bill);
        });

        group.MapPost("/", async (BillReminder bill, IBillService svc, CancellationToken ct) =>
        {
            var created = await svc.AddBillAsync(bill, ct);
            return Results.Created($"/api/bills/{created.Id}", created);
        });

        group.MapPut("/{id:long}", async (long id, BillReminder input, IBillService svc, CancellationToken ct) =>
        {
            var existing = await svc.GetBillByIdAsync(id, ct);
            if (existing is null) return Results.NotFound();
            existing.Name = input.Name;
            existing.Amount = input.Amount;
            existing.Category = input.Category;
            existing.CycleType = input.CycleType;
            existing.StartDate = input.StartDate;
            existing.IsActive = input.IsActive;
            existing.MemberId = input.MemberId;
            existing.Notes = input.Notes;
            await svc.UpdateBillAsync(existing, ct);
            return Results.Ok(existing);
        });

        group.MapPost("/{id:long}/paid", async (long id, IBillService svc, CancellationToken ct) =>
        {
            await svc.MarkPaidAsync(id, ct);
            return Results.Ok(new { paid = true });
        });

        group.MapDelete("/{id:long}", async (long id, IBillService svc, CancellationToken ct) =>
        {
            await svc.DeleteBillAsync(id, ct);
            return Results.NoContent();
        });
    }
}
