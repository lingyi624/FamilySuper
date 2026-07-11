using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 学习报告
/// </summary>
/// <param name="TotalMinutes">总分钟数</param>
/// <param name="TotalSessions">总场次</param>
/// <param name="MinutesBySubject">按科目统计分钟数</param>
/// <param name="MinutesByDay">按天统计分钟数</param>
/// <param name="CompletionRate">完成率</param>
/// <param name="RecentLogs">最近学习记录</param>
public record StudyReport(
    int TotalMinutes,
    int TotalSessions,
    Dictionary<string, int> MinutesBySubject,
    Dictionary<string, int> MinutesByDay,
    double CompletionRate,
    List<StudyLog> RecentLogs);

/// <summary>
/// 学习服务接口
/// </summary>
public interface IStudyService
{
    /// <summary>
    /// 获取学习计划列表
    /// </summary>
    /// <param name="memberId">成员ID筛选</param>
    /// <param name="activeOnly">是否只获取激活的</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>学习计划列表</returns>
    Task<List<StudyPlan>> GetPlansAsync(long? memberId = null, bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取学习计划
    /// </summary>
    /// <param name="id">计划ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>学习计划</returns>
    Task<StudyPlan?> GetPlanByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加学习计划
    /// </summary>
    /// <param name="plan">学习计划</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新计划</returns>
    Task<StudyPlan> AddPlanAsync(StudyPlan plan, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新学习计划
    /// </summary>
    /// <param name="plan">学习计划</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdatePlanAsync(StudyPlan plan, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除学习计划
    /// </summary>
    /// <param name="id">计划ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeletePlanAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取学习记录列表
    /// </summary>
    /// <param name="planId">计划ID筛选</param>
    /// <param name="from">起始日期</param>
    /// <param name="to">结束日期</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>学习记录列表</returns>
    Task<List<StudyLog>> GetLogsAsync(long? planId = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 记录学习时间
    /// </summary>
    /// <param name="log">学习记录</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新记录</returns>
    Task<StudyLog> LogStudyAsync(StudyLog log, CancellationToken cancellationToken = default);

    /// <summary>
    /// 生成学习报告
    /// </summary>
    /// <param name="memberId">成员ID</param>
    /// <param name="from">起始日期</param>
    /// <param name="to">结束日期</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>学习报告</returns>
    Task<StudyReport> GenerateReportAsync(long? memberId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
