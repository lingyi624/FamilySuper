using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 家庭媒体服务接口
/// </summary>
public interface IMediaItemService
{
    /// <summary>
    /// 获取媒体列表
    /// </summary>
    /// <param name="mediaType">媒体类型筛选</param>
    /// <param name="category">分类筛选</param>
    /// <param name="favoriteOnly">只看收藏</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>媒体列表</returns>
    Task<List<MediaItem>> GetItemsAsync(string? mediaType = null, string? category = null, bool favoriteOnly = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取媒体
    /// </summary>
    /// <param name="id">媒体ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>媒体实体</returns>
    Task<MediaItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加媒体
    /// </summary>
    /// <param name="item">媒体实体</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的媒体</returns>
    Task<MediaItem> AddAsync(MediaItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新媒体
    /// </summary>
    /// <param name="item">媒体实体</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateAsync(MediaItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除媒体
    /// </summary>
    /// <param name="id">媒体ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 切换收藏状态
    /// </summary>
    /// <param name="id">媒体ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task ToggleFavoriteAsync(long id, CancellationToken cancellationToken = default);
}