using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 家庭物品服务接口
/// </summary>
public interface IHomeItemService
{
    /// <summary>
    /// 获取物品列表
    /// </summary>
    /// <param name="category">分类筛选</param>
    /// <param name="status">状态筛选</param>
    /// <param name="location">位置筛选</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>物品列表</returns>
    Task<List<HomeItem>> GetItemsAsync(string? category = null, string? status = null, string? location = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取物品
    /// </summary>
    /// <param name="id">物品ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>物品实体</returns>
    Task<HomeItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加物品
    /// </summary>
    /// <param name="item">物品实体</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的物品</returns>
    Task<HomeItem> AddAsync(HomeItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新物品
    /// </summary>
    /// <param name="item">物品实体</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateAsync(HomeItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除物品
    /// </summary>
    /// <param name="id">物品ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}