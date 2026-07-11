using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 健康服务接口
/// </summary>
public interface IHealthService
{
    /// <summary>
    /// 获取健康记录列表
    /// </summary>
    /// <param name="memberId">成员ID筛选</param>
    /// <param name="type">记录类型筛选</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>健康记录列表</returns>
    Task<List<HealthRecord>> GetRecordsAsync(long? memberId = null, HealthRecordType? type = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取健康记录
    /// </summary>
    /// <param name="id">记录ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>健康记录</returns>
    Task<HealthRecord?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加健康记录
    /// </summary>
    /// <param name="record">健康记录</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的健康记录</returns>
    Task<HealthRecord> AddAsync(HealthRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新健康记录
    /// </summary>
    /// <param name="record">健康记录</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateAsync(HealthRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除健康记录
    /// </summary>
    /// <param name="id">记录ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
