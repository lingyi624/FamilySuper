using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FamilySuper.API.Endpoints;

public static class HealthRecordsEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/health-records").WithTags("HealthRecords");

        group.MapGet("/records", async (IHealthService svc, long? memberId, HealthRecordType? type, CancellationToken ct) =>
            Results.Ok(await svc.GetRecordsAsync(memberId, type, ct)));

        group.MapGet("/records/{id:long}", async (long id, IHealthService svc, CancellationToken ct) =>
        {
            var record = await svc.GetByIdAsync(id, ct);
            return record is null ? Results.NotFound() : Results.Ok(record);
        });

        group.MapPost("/records", async (HealthRecord record, IHealthService svc, CancellationToken ct) =>
        {
            var created = await svc.AddAsync(record, ct);
            return Results.Created($"/api/health-records/records/{created.Id}", created);
        });

        group.MapPut("/records/{id:long}", async (long id, HealthRecord input, IHealthService svc, CancellationToken ct) =>
        {
            var existing = await svc.GetByIdAsync(id, ct);
            if (existing is null) return Results.NotFound();

            existing.Title = input.Title;
            existing.RecordType = input.RecordType;
            existing.Content = input.Content;
            existing.RecordDate = input.RecordDate;
            existing.Doctor = input.Doctor;
            existing.Hospital = input.Hospital;
            existing.Remark = input.Remark;
            await svc.UpdateAsync(existing, ct);
            return Results.Ok(existing);
        });

        group.MapDelete("/records/{id:long}", async (long id, IHealthService svc, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }
}
