using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 养老关怀服务接口
/// </summary>
public interface IElderlyCareService
{
    /// <summary>
    /// 获取紧急联络人列表
    /// </summary>
    /// <param name="memberId">成员ID筛选</param>
    /// <returns>紧急联络人列表</returns>
    Task<List<EmergencyContact>> GetContactsAsync(long? memberId = null);

    /// <summary>
    /// 根据ID获取紧急联络人
    /// </summary>
    /// <param name="id">联络人ID</param>
    /// <returns>紧急联络人</returns>
    Task<EmergencyContact?> GetContactByIdAsync(long id);

    /// <summary>
    /// 添加紧急联络人
    /// </summary>
    /// <param name="contact">紧急联络人</param>
    /// <returns>新增的紧急联络人</returns>
    Task<EmergencyContact> AddContactAsync(EmergencyContact contact);

    /// <summary>
    /// 更新紧急联络人
    /// </summary>
    /// <param name="contact">紧急联络人</param>
    /// <returns>更新后的紧急联络人</returns>
    Task<EmergencyContact> UpdateContactAsync(EmergencyContact contact);

    /// <summary>
    /// 删除紧急联络人
    /// </summary>
    /// <param name="id">联络人ID</param>
    Task DeleteContactAsync(long id);

    /// <summary>
    /// 获取健康指标列表
    /// </summary>
    /// <param name="memberId">成员ID筛选</param>
    /// <param name="from">起始日期</param>
    /// <param name="to">结束日期</param>
    /// <returns>健康指标列表</returns>
    Task<List<HealthMetric>> GetMetricsAsync(long? memberId = null, DateTime? from = null, DateTime? to = null);

    /// <summary>
    /// 根据ID获取健康指标
    /// </summary>
    /// <param name="id">指标ID</param>
    /// <returns>健康指标</returns>
    Task<HealthMetric?> GetMetricByIdAsync(long id);

    /// <summary>
    /// 添加健康指标
    /// </summary>
    /// <param name="metric">健康指标</param>
    /// <returns>新增的健康指标</returns>
    Task<HealthMetric> AddMetricAsync(HealthMetric metric);

    /// <summary>
    /// 删除健康指标
    /// </summary>
    /// <param name="id">指标ID</param>
    Task DeleteMetricAsync(long id);
}
