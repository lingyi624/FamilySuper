using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FamilySuper.API.Endpoints;

public static class SurveyEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/surveys").WithTags("Surveys");

        group.MapGet("/", async (ISurveyService svc, bool? activeOnly, CancellationToken ct) =>
            Results.Ok(await svc.GetSurveysAsync(activeOnly, ct)));

        group.MapGet("/{id:long}", async (long id, ISurveyService svc, CancellationToken ct) =>
        {
            var survey = await svc.GetSurveyByIdAsync(id, ct);
            return survey is null ? Results.NotFound() : Results.Ok(survey);
        });

        group.MapPost("/", async (Survey survey, ISurveyService svc, CancellationToken ct) =>
        {
            var created = await svc.AddSurveyAsync(survey, ct);
            return Results.Created($"/api/surveys/{created.Id}", created);
        });

        group.MapPut("/{id:long}", async (long id, Survey input, ISurveyService svc, CancellationToken ct) =>
        {
            var existing = await svc.GetSurveyByIdAsync(id, ct);
            if (existing is null) return Results.NotFound();
            existing.Title = input.Title;
            existing.Description = input.Description;
            existing.FieldsJson = input.FieldsJson;
            existing.StartDate = input.StartDate;
            existing.EndDate = input.EndDate;
            existing.IsActive = input.IsActive;
            await svc.UpdateSurveyAsync(existing, ct);
            return Results.Ok(existing);
        });

        group.MapDelete("/{id:long}", async (long id, ISurveyService svc, CancellationToken ct) =>
        {
            await svc.DeleteSurveyAsync(id, ct);
            return Results.NoContent();
        });

        group.MapPost("/{id:long}/responses", async (long id, SurveyResponse response, ISurveyService svc, CancellationToken ct) =>
        {
            response.SurveyId = id;
            var created = await svc.SubmitResponseAsync(response, ct);
            return Results.Created($"/api/surveys/{id}/responses/{created.Id}", created);
        });

        group.MapGet("/{id:long}/responses", async (long id, ISurveyService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetResponsesAsync(id, ct)));

        group.MapGet("/{id:long}/statistics", async (long id, ISurveyService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetStatisticAsync(id, ct)));
    }
}
