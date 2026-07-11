using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FamilySuper.API.Endpoints;

public static class ElderlyCareEndpoints
{
    public static void MapElderlyCareEndpoints(this IEndpointRouteBuilder app)
    {
        var contacts = app.MapGroup("/api/elderly-care/contacts");
        contacts.MapGet("/", async (IElderlyCareService svc, long? memberId) =>
            Results.Ok(await svc.GetContactsAsync(memberId)));
        contacts.MapGet("/{id:long}", async (IElderlyCareService svc, long id) =>
        {
            var c = await svc.GetContactByIdAsync(id);
            return c is not null ? Results.Ok(c) : Results.NotFound();
        });
        contacts.MapPost("/", async (IElderlyCareService svc, EmergencyContact contact) =>
        {
            var created = await svc.AddContactAsync(contact);
            return Results.Created($"/api/elderly-care/contacts/{created.Id}", created);
        });
        contacts.MapPut("/{id:long}", async (IElderlyCareService svc, long id, EmergencyContact contact) =>
        {
            contact.Id = id;
            return Results.Ok(await svc.UpdateContactAsync(contact));
        });
        contacts.MapDelete("/{id:long}", async (IElderlyCareService svc, long id) =>
        {
            await svc.DeleteContactAsync(id);
            return Results.NoContent();
        });

        var metrics = app.MapGroup("/api/elderly-care/metrics");
        metrics.MapGet("/", async (IElderlyCareService svc, long? memberId, DateTime? from, DateTime? to) =>
            Results.Ok(await svc.GetMetricsAsync(memberId, from, to)));
        metrics.MapPost("/", async (IElderlyCareService svc, HealthMetric metric) =>
        {
            var created = await svc.AddMetricAsync(metric);
            return Results.Created($"/api/elderly-care/metrics/{created.Id}", created);
        });
        metrics.MapDelete("/{id:long}", async (IElderlyCareService svc, long id) =>
        {
            await svc.DeleteMetricAsync(id);
            return Results.NoContent();
        });
    }
}