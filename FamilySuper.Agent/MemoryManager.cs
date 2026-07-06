using System.Text.Json;
using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using FamilySuper.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Agent;

public interface IMemoryManager
{
    Task AddMemoryAsync(string content, string category, string mode, string? memberId = null);
    Task<List<MemoryItem>> SearchAsync(string query, string mode, int limit = 5);
}

public record MemoryItem(long Id, string Content, string Category, string Mode, string? MemberId, DateTime CreatedAt);

public class MemoryManager : IMemoryManager
{
    private readonly IServiceProvider _sp;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<MemoryManager> _logger;

    public MemoryManager(IServiceProvider sp, IEmbeddingService embeddingService, ILogger<MemoryManager> logger)
    {
        _sp = sp;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task AddMemoryAsync(string content, string category, string mode, string? memberId = null)
    {
        using var scope = _sp.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<MemoryEntry>>();

        long? memberIdLong = null;
        if (long.TryParse(memberId, out var mid)) memberIdLong = mid;

        string embeddingJson = string.Empty;
        if (_embeddingService.IsAvailable)
        {
            try
            {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(content);
                embeddingJson = JsonSerializer.Serialize(embedding);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "生成嵌入向量失败,记忆将仅支持关键词检索");
            }
        }

        var entry = new MemoryEntry
        {
            Content = content,
            Category = category,
            Mode = mode,
            MemberId = memberIdLong,
            EmbeddingJson = embeddingJson,
            CreatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(entry);
        await repo.SaveChangesAsync();
        _logger.LogDebug("记忆已持久化: {Category} (mode={Mode}, id={Id}, vector={HasVector})", category, mode, entry.Id, embeddingJson.Length > 0);
    }

    public async Task<List<MemoryItem>> SearchAsync(string query, string mode, int limit = 5)
    {
        using var scope = _sp.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<MemoryEntry>>();
        var entries = await repo.FindAsync(m => m.Mode == mode);

        if (!string.IsNullOrWhiteSpace(query) && _embeddingService.IsAvailable)
        {
            try
            {
                var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);
                var scored = entries
                    .Where(e => !string.IsNullOrEmpty(e.EmbeddingJson))
                    .Select(e => (Entry: e, Score: CosineSimilarity(queryEmbedding, DeserializeEmbedding(e.EmbeddingJson!))))
                    .OrderByDescending(x => x.Score)
                    .Take(limit)
                    .Select(x => new MemoryItem(x.Entry.Id, x.Entry.Content, x.Entry.Category, x.Entry.Mode, x.Entry.MemberId?.ToString(), x.Entry.CreatedAt))
                    .ToList();

                if (scored.Count > 0) return scored;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "向量检索失败,回退到关键词检索");
            }
        }

        return entries
            .Where(e => string.IsNullOrWhiteSpace(query) || e.Content.Contains(query))
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .Select(m => new MemoryItem(m.Id, m.Content, m.Category, m.Mode, m.MemberId?.ToString(), m.CreatedAt))
            .ToList();
    }

    private static float[] DeserializeEmbedding(string json)
        => JsonSerializer.Deserialize<float[]>(json) ?? Array.Empty<float>();

    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length == 0 || b.Length == 0 || a.Length != b.Length) return 0f;
        float dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }
        var denom = Math.Sqrt(magA) * Math.Sqrt(magB);
        return denom == 0 ? 0f : dot / (float)denom;
    }
}
