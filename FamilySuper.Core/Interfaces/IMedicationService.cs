using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 用药服务接口
/// </summary>
public interface IMedicationService
{
    /// <summary>
    /// 获取用药计划列表
    /// </summary>
    /// <param name="memberId">成员ID筛选</param>
    /// <param name="activeOnly">是否只获取激活的</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用药计划列表</returns>
    Task<List<MedicationPlan>> GetPlansAsync(long? memberId = null, bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取用药计划
    /// </summary>
    /// <param name="id">计划ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用药计划</returns>
    Task<MedicationPlan?> GetPlanByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加用药计划
    /// </summary>
    /// <param name="plan">用药计划</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的用药计划</returns>
    Task<MedicationPlan> AddPlanAsync(MedicationPlan plan, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新用药计划
    /// </summary>
    /// <param name="plan">用药计划</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdatePlanAsync(MedicationPlan plan, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除用药计划
    /// </summary>
    /// <param name="id">计划ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeletePlanAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用药记录列表
    /// </summary>
    /// <param name="planId">计划ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用药记录列表</returns>
    Task<List<MedicationRecord>> GetRecordsAsync(long planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 记录用药剂量
    /// </summary>
    /// <param name="record">用药记录</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的用药记录</returns>
    Task<MedicationRecord> RecordDoseAsync(MedicationRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取即将到期的提醒
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用药计划列表</returns>
    Task<List<MedicationPlan>> GetDueRemindersAsync(CancellationToken cancellationToken = default);
}
