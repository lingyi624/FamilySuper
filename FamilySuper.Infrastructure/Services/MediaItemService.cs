using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class MediaItemService : IMediaItemService
{
    private readonly IRepository<MediaItem> _repo;
    private readonly ILogger<MediaItemService> _logger;

    public MediaItemService(IRepository<MediaItem> repo, ILogger<MediaItemService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<MediaItem>> GetItemsAsync(string? mediaType = null, string? category = null, bool favoriteOnly = false, CancellationToken cancellationToken = default)
    {
        var items = await _repo.GetAllAsync(cancellationToken);
        if (!string.IsNullOrEmpty(mediaType))
            items = items.Where(i => i.MediaType == mediaType).ToList();
        if (!string.IsNullOrEmpty(category))
            items = items.Where(i => i.Category == category).ToList();
        if (favoriteOnly)
            items = items.Where(i => i.IsFavorite).ToList();
        return items.OrderByDescending(i => i.CreatedAt).ToList();
    }

    public async Task<MediaItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    public async Task<MediaItem> AddAsync(MediaItem item, CancellationToken cancellationToken = default)
    {
        item.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(item, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增媒体:{Title}", item.Title);
        return item;
    }

    public async Task UpdateAsync(MediaItem item, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetByIdAsync(item.Id, cancellationToken)
            ?? throw new InvalidOperationException("媒体不存在");
        existing.Title = item.Title;
        existing.FilePath = item.FilePath;
        existing.MediaType = item.MediaType;
        existing.Category = item.Category;
        existing.Tags = item.Tags;
        existing.FileSize = item.FileSize;
        existing.TakenDate = item.TakenDate;
        existing.Description = item.Description;
        existing.IsFavorite = item.IsFavorite;
        existing.MemberId = item.MemberId;
        existing.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(existing, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await _repo.GetByIdAsync(id, cancellationToken);
        if (item is null) return;
        await _repo.DeleteAsync(item, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task ToggleFavoriteAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await _repo.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("媒体不存在");
        item.IsFavorite = !item.IsFavorite;
        item.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(item, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }
}