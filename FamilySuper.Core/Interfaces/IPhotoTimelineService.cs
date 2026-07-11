using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 照片时间墙服务接口
/// 提供照片时间墙的增删改查功能
/// </summary>
public interface IPhotoTimelineService
{
    /// <summary>
    /// 获取所有照片时间线记录
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>照片时间线列表</returns>
    Task<List<PhotoTimeline>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据分类获取照片
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>照片时间线列表</returns>
    Task<List<PhotoTimeline>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据年份获取照片
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>照片时间线列表</returns>
    Task<List<PhotoTimeline>> GetByYearAsync(int year, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据年月获取照片
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="month">月份</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>照片时间线列表</returns>
    Task<List<PhotoTimeline>> GetByMonthAsync(int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取照片
    /// </summary>
    /// <param name="id">照片ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>照片时间线</returns>
    Task<PhotoTimeline?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加照片
    /// </summary>
    /// <param name="photo">照片实体</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>添加后的照片实体</returns>
    Task<PhotoTimeline> AddAsync(PhotoTimeline photo, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新照片
    /// </summary>
    /// <param name="photo">照片实体</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    Task UpdateAsync(PhotoTimeline photo, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除照片
    /// </summary>
    /// <param name="id">照片ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取统计信息（按年份分组）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>年份统计字典</returns>
    Task<Dictionary<int, int>> GetYearStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有分类
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>分类列表</returns>
    Task<List<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}
