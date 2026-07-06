using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FamilySuper.API.Endpoints;

public static class FinanceEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/finance").WithTags("Finance");

        group.MapGet("/records", async (IFinanceService svc, FinanceType? type, string? category, CancellationToken ct) =>
            Results.Ok(await svc.GetRecordsAsync(type, category, ct)));

        group.MapGet("/records/{id:long}", async (long id, IFinanceService svc, CancellationToken ct) =>
        {
            var record = await svc.GetByIdAsync(id, ct);
            return record is null ? Results.NotFound() : Results.Ok(record);
        });

        group.MapPost("/records", async (FinanceRecord record, IFinanceService svc, CancellationToken ct) =>
        {
            var created = await svc.AddAsync(record, ct);
            return Results.Created($"/api/finance/records/{created.Id}", created);
        });

        group.MapPut("/records/{id:long}", async (long id, FinanceRecord input, IFinanceService svc, CancellationToken ct) =>
        {
            var existing = await svc.GetByIdAsync(id, ct);
            if (existing is null) return Results.NotFound();

            existing.Title = input.Title;
            existing.Type = input.Type;
            existing.Amount = input.Amount;
            existing.Category = input.Category;
            existing.RecordDate = input.RecordDate;
            existing.Remark = input.Remark;
            await svc.UpdateAsync(existing, ct);
            return Results.Ok(existing);
        });

        group.MapDelete("/records/{id:long}", async (long id, IFinanceService svc, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });

        group.MapGet("/summary", async (IFinanceService svc, DateTime? startDate, DateTime? endDate, CancellationToken ct) =>
            Results.Ok(await svc.GetSummaryAsync(startDate, endDate, ct)));
    }
}
