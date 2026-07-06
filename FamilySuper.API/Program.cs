var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "FamilySuper API - use /health to check status");
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTimeOffset.UtcNow });

app.Run();
