using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FamilySuper.API.Endpoints;

public static class ShoppingEndpoints
{
    public static void MapShoppingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/shopping");

        group.MapGet("/", async (IShoppingService svc, [FromQuery] string? status, [FromQuery] long? memberId) =>
        {
            ShoppingStatus? statusFilter = status is not null && Enum.TryParse<ShoppingStatus>(status, true, out var s) ? s : null;
            var items = await svc.GetItemsAsync(statusFilter, memberId);
            return Results.Ok(items);
        });

        group.MapGet("/{id:long}", async (IShoppingService svc, long id) =>
        {
            var item = await svc.GetByIdAsync(id);
            return item is not null ? Results.Ok(item) : Results.NotFound();
        });

        group.MapPost("/", async (IShoppingService svc, ShoppingItem item) =>
        {
            var created = await svc.AddAsync(item);
            return Results.Created($"/api/shopping/{created.Id}", created);
        });

        group.MapPut("/{id:long}", async (IShoppingService svc, long id, ShoppingItem item) =>
        {
            item.Id = id;
            var updated = await svc.UpdateAsync(item);
            return Results.Ok(updated);
        });

        group.MapDelete("/{id:long}", async (IShoppingService svc, long id) =>
        {
            await svc.DeleteAsync(id);
            return Results.NoContent();
        });

        group.MapPost("/{id:long}/purchase", async (IShoppingService svc, long id) =>
        {
            await svc.MarkPurchasedAsync(id);
            return Results.Ok();
        });

        group.MapGet("/suggestions", async (IShoppingService svc) =>
        {
            var suggestions = await svc.SuggestPurchasesAsync();
            return Results.Ok(suggestions);
        });
    }
}