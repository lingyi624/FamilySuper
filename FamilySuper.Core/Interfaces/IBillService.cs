using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 账单服务接口
/// </summary>
public interface IBillService
{
    /// <summary>
    /// 获取账单列表
    /// </summary>
    /// <param name="activeOnly">是否只获取激活的</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>账单列表</returns>
    Task<List<BillReminder>> GetBillsAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取账单
    /// </summary>
    /// <param name="id">账单ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>账单</returns>
    Task<BillReminder?> GetBillByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加账单
    /// </summary>
    /// <param name="bill">账单</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的账单</returns>
    Task<BillReminder> AddBillAsync(BillReminder bill, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新账单
    /// </summary>
    /// <param name="bill">账单</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateBillAsync(BillReminder bill, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除账单
    /// </summary>
    /// <param name="id">账单ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteBillAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记为已支付
    /// </summary>
    /// <param name="id">账单ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task MarkPaidAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取即将到期的账单
    /// </summary>
    /// <param name="days">天数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>账单列表</returns>
    Task<List<BillReminder>> GetDueSoonAsync(int days = 7, CancellationToken cancellationToken = default);
}
