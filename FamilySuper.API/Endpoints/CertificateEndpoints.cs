using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FamilySuper.API.Endpoints;

public static class CertificateEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/certificates").WithTags("Certificate");

        group.MapGet("/", async (ICertificateService svc, long? memberId, CancellationToken ct) =>
            Results.Ok(await svc.GetByMemberAsync(memberId, ct)));

        group.MapGet("/{id:long}", async (long id, ICertificateService svc, CancellationToken ct) =>
        {
            var cert = await svc.GetByIdAsync(id, ct);
            return cert is null ? Results.NotFound() : Results.Ok(cert);
        });

        group.MapPost("/", async (HttpContext context, ICertificateService svc, CancellationToken ct) =>
        {
            var form = await context.Request.ReadFormAsync(ct);
            var memberIdStr = form["memberId"].ToString();
            var name = form["name"].ToString();
            var typeStr = form["type"].ToString();
            var number = form["number"].ToString();
            var remark = form["remark"].ToString();

            long.TryParse(memberIdStr, out var memberId);
            Enum.TryParse<CertificateType>(typeStr, out var certType);

            var certificate = new Certificate
            {
                MemberId = memberId,
                Name = name,
                Type = certType,
                Number = number,
                Remark = remark,
                FileName = string.Empty,
                FileSize = 0
            };

            byte[]? fileBytes = null;
            var file = form.Files.FirstOrDefault();
            if (file is not null && file.Length > 0)
            {
                certificate.FileName = file.FileName;
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, ct);
                fileBytes = ms.ToArray();
            }

            var created = await svc.AddAsync(certificate, fileBytes, ct);
            return Results.Created($"/api/certificates/{created.Id}", created);
        });

        group.MapGet("/{id:long}/file", async (long id, ICertificateService svc, CancellationToken ct) =>
        {
            var fileData = await svc.GetFileAsync(id, ct);
            if (fileData is null) return Results.NotFound();

            var (content, fileName) = fileData.Value;
            return Results.File(content, "application/octet-stream", fileName);
        });

        group.MapDelete("/{id:long}", async (long id, ICertificateService svc, CancellationToken ct) =>
        {
            await svc.DeleteAsync(id, ct);
            return Results.NoContent();
        });
    }
}
