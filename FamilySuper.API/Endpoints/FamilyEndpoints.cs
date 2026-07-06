using FamilySuper.Core.Entities;
using FamilySuper.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FamilySuper.API.Endpoints;

public static class FamilyEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/family").WithTags("Family");

        group.MapGet("/members", async (FamilyDbContext db, CancellationToken ct) =>
        {
            var members = await db.FamilyMembers.ToListAsync(ct);
            return Results.Ok(members);
        });

        group.MapGet("/members/{id:long}", async (long id, FamilyDbContext db, CancellationToken ct) =>
        {
            var member = await db.FamilyMembers.FindAsync(new object[] { id }, ct);
            return member is null ? Results.NotFound() : Results.Ok(member);
        });

        group.MapPost("/members", async (FamilyMember member, FamilyDbContext db, CancellationToken ct) =>
        {
            member.CreatedAt = DateTime.UtcNow;
            member.UpdatedAt = DateTime.UtcNow;
            db.FamilyMembers.Add(member);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/family/members/{member.Id}", member);
        });

        group.MapPut("/members/{id:long}", async (long id, FamilyMember input, FamilyDbContext db, CancellationToken ct) =>
        {
            var member = await db.FamilyMembers.FindAsync(new object[] { id }, ct);
            if (member is null) return Results.NotFound();

            member.Name = input.Name;
            member.Role = input.Role;
            member.Phone = input.Phone;
            member.Address = input.Address;
            member.BirthDate = input.BirthDate;
            member.Gender = input.Gender;
            member.Preferences = input.Preferences;
            member.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
            return Results.Ok(member);
        });

        group.MapDelete("/members/{id:long}", async (long id, FamilyDbContext db, CancellationToken ct) =>
        {
            var member = await db.FamilyMembers.FindAsync(new object[] { id }, ct);
            if (member is null) return Results.NotFound();

            member.IsDeleted = true;
            member.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }
}
