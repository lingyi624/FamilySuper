using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

/// <summary>
/// 照片时间墙服务
/// 提供照片时间墙的增删改查功能
/// </summary>
public class PhotoTimelineService : IPhotoTimelineService
{
    private readonly IRepository<PhotoTimeline> _repo;
    private readonly ILogger<PhotoTimelineService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="repo">照片时间线仓储</param>
    /// <param name="logger">日志记录器</param>
    public PhotoTimelineService(IRepository<PhotoTimeline> repo, ILogger<PhotoTimelineService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有照片时间线记录
    /// </summary>
    public async Task<List<PhotoTimeline>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var photos = await _repo.GetAllAsync(cancellationToken);
        return photos.OrderByDescending(p => p.TakenDate ?? p.CreatedAt).ToList();
    }

    /// <summary>
    /// 根据分类获取照片
    /// </summary>
    public async Task<List<PhotoTimeline>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var photos = await _repo.GetAllAsync(cancellationToken);
        return photos.Where(p => p.Category == category)
            .OrderByDescending(p => p.TakenDate ?? p.CreatedAt).ToList();
    }

    /// <summary>
    /// 根据年份获取照片
    /// </summary>
    public async Task<List<PhotoTimeline>> GetByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var photos = await _repo.GetAllAsync(cancellationToken);
        return photos.Where(p => (p.TakenDate ?? p.CreatedAt).Year == year)
            .OrderByDescending(p => p.TakenDate ?? p.CreatedAt).ToList();
    }

    /// <summary>
    /// 根据年月获取照片
    /// </summary>
    public async Task<List<PhotoTimeline>> GetByMonthAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var photos = await _repo.GetAllAsync(cancellationToken);
        return photos.Where(p => 
            (p.TakenDate ?? p.CreatedAt).Year == year && 
            (p.TakenDate ?? p.CreatedAt).Month == month)
            .OrderByDescending(p => p.TakenDate ?? p.CreatedAt).ToList();
    }

    /// <summary>
    /// 根据ID获取照片
    /// </summary>
    public async Task<PhotoTimeline?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    /// <summary>
    /// 添加照片
    /// </summary>
    public async Task<PhotoTimeline> AddAsync(PhotoTimeline photo, CancellationToken cancellationToken = default)
    {
        photo.CreatedAt = DateTime.UtcNow;
        photo.UpdatedAt = DateTime.UtcNow;
        await _repo.AddAsync(photo, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增照片时间墙记录: {Id}, {Title}", photo.Id, photo.Title);
        return photo;
    }

    /// <summary>
    /// 更新照片
    /// </summary>
    public async Task UpdateAsync(PhotoTimeline photo, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetByIdAsync(photo.Id, cancellationToken)
            ?? throw new InvalidOperationException("照片不存在");
        
        existing.Title = photo.Title;
        existing.FilePath = photo.FilePath;
        existing.ThumbnailPath = photo.ThumbnailPath;
        existing.Description = photo.Description;
        existing.TakenDate = photo.TakenDate;
        existing.Location = photo.Location;
        existing.Latitude = photo.Latitude;
        existing.Longitude = photo.Longitude;
        existing.PeopleTags = photo.PeopleTags;
        existing.SceneTags = photo.SceneTags;
        existing.Category = photo.Category;
        existing.MemberId = photo.MemberId;
        existing.IsCover = photo.IsCover;
        existing.UpdatedAt = DateTime.UtcNow;
        
        await _repo.UpdateAsync(existing, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("更新照片时间墙记录: {Id}", photo.Id);
    }

    /// <summary>
    /// 删除照片
    /// </summary>
    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var photo = await _repo.GetByIdAsync(id, cancellationToken);
        if (photo == null) return;

        await _repo.DeleteAsync(photo, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        
        if (!string.IsNullOrEmpty(photo.FilePath) && File.Exists(photo.FilePath))
        {
            File.Delete(photo.FilePath);
        }
        if (!string.IsNullOrEmpty(photo.ThumbnailPath) && File.Exists(photo.ThumbnailPath))
        {
            File.Delete(photo.ThumbnailPath);
        }
        
        _logger.LogInformation("删除照片时间墙记录: {Id}", id);
    }

    /// <summary>
    /// 获取统计信息（按年份分组）
    /// </summary>
    public async Task<Dictionary<int, int>> GetYearStatsAsync(CancellationToken cancellationToken = default)
    {
        var photos = await _repo.GetAllAsync(cancellationToken);
        return photos.GroupBy(p => (p.TakenDate ?? p.CreatedAt).Year)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// 获取所有分类
    /// </summary>
    public async Task<List<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var photos = await _repo.GetAllAsync(cancellationToken);
        return photos.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
    }
}
