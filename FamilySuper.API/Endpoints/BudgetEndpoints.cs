using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FamilySuper.API.Endpoints;

public static class BudgetEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/budgets").WithTags("Budgets");

        group.MapGet("/", async (IBudgetService svc, int? year, int? month, CancellationToken ct) =>
            Results.Ok(await svc.GetBudgetsAsync(year, month, ct)));

        group.MapGet("/status/{year:int}/{month:int}", async (int year, int month, IBudgetService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetMonthlyStatusAsync(year, month, ct)));

        group.MapPost("/", async (Budget budget, IBudgetService svc, CancellationToken ct) =>
        {
            var saved = await svc.AddOrUpdateAsync(budget, ct);
            return Results.Created($"/api/budgets/{saved.Id}", saved);
        });

        group.MapDelete("/{id:long}", async (long id, IBudgetService svc, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }
}
