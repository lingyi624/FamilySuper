using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 工作任务服务接口
/// </summary>
public interface IWorkTaskService
{
    /// <summary>
    /// 获取任务列表
    /// </summary>
    /// <param name="status">状态筛选</param>
    /// <param name="priority">优先级筛选</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务列表</returns>
    Task<List<WorkTask>> GetTasksAsync(Enums.TaskStatus? status = null, TaskPriority? priority = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取任务
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    Task<WorkTask?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加任务
    /// </summary>
    /// <param name="task">任务</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的任务</returns>
    Task<WorkTask> AddAsync(WorkTask task, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新任务
    /// </summary>
    /// <param name="task">任务</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateAsync(WorkTask task, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新任务状态
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <param name="status">新状态</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateStatusAsync(long id, Enums.TaskStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除任务
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
