using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 比价助手服务接口
/// </summary>
public interface IPriceCompareService
{
    /// <summary>
    /// 获取比价记录列表
    /// </summary>
    /// <param name="productName">商品名称筛选</param>
    /// <param name="category">分类筛选</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>比价记录列表</returns>
    Task<List<PriceCompare>> GetItemsAsync(string? productName = null, string? category = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取比价记录
    /// </summary>
    /// <param name="id">记录ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>比价记录</returns>
    Task<PriceCompare?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加比价记录
    /// </summary>
    /// <param name="item">比价记录</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的比价记录</returns>
    Task<PriceCompare> AddAsync(PriceCompare item, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新比价记录
    /// </summary>
    /// <param name="item">比价记录</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateAsync(PriceCompare item, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除比价记录
    /// </summary>
    /// <param name="id">记录ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取按商品分组的最低价格汇总
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>最低价汇总列表</returns>
    Task<List<PriceCompare>> GetLowestPricesAsync(CancellationToken cancellationToken = default);
}