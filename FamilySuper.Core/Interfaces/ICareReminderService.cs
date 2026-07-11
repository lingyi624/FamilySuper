using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 关怀提醒服务接口
/// </summary>
public interface ICareReminderService
{
    /// <summary>
    /// 获取所有提醒
    /// </summary>
    /// <param name="isCompleted">完成状态筛选</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>提醒列表</returns>
    Task<List<CareReminder>> GetAllAsync(bool? isCompleted = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取提醒
    /// </summary>
    /// <param name="id">提醒ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>提醒实体</returns>
    Task<CareReminder?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加提醒
    /// </summary>
    /// <param name="reminder">提醒实体</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的提醒</returns>
    Task<CareReminder> AddAsync(CareReminder reminder, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新提醒
    /// </summary>
    /// <param name="reminder">提醒实体</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateAsync(CareReminder reminder, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除提醒
    /// </summary>
    /// <param name="id">提醒ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记为完成
    /// </summary>
    /// <param name="id">提醒ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task MarkCompletedAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取即将到期的提醒
    /// </summary>
    /// <param name="days">天数范围</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>即将到期的提醒列表</returns>
    Task<List<CareReminder>> GetDueSoonAsync(int days = 3, CancellationToken cancellationToken = default);
}