using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 购物服务接口
/// </summary>
public interface IShoppingService
{
    /// <summary>
    /// 获取购物项列表
    /// </summary>
    /// <param name="status">状态筛选</param>
    /// <param name="memberId">成员ID筛选</param>
    /// <returns>购物项列表</returns>
    Task<List<ShoppingItem>> GetItemsAsync(ShoppingStatus? status = null, long? memberId = null);

    /// <summary>
    /// 根据ID获取购物项
    /// </summary>
    /// <param name="id">购物项ID</param>
    /// <returns>购物项</returns>
    Task<ShoppingItem?> GetByIdAsync(long id);

    /// <summary>
    /// 添加购物项
    /// </summary>
    /// <param name="item">购物项</param>
    /// <returns>新增的购物项</returns>
    Task<ShoppingItem> AddAsync(ShoppingItem item);

    /// <summary>
    /// 更新购物项
    /// </summary>
    /// <param name="item">购物项</param>
    /// <returns>更新后的购物项</returns>
    Task<ShoppingItem> UpdateAsync(ShoppingItem item);

    /// <summary>
    /// 删除购物项
    /// </summary>
    /// <param name="id">购物项ID</param>
    Task DeleteAsync(long id);

    /// <summary>
    /// 标记为已购买
    /// </summary>
    /// <param name="id">购物项ID</param>
    Task MarkPurchasedAsync(long id);

    /// <summary>
    /// 获取智能推荐购买清单
    /// </summary>
    /// <returns>推荐商品列表</returns>
    Task<List<string>> SuggestPurchasesAsync();
}
