using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 教育服务接口
/// </summary>
public interface IEducationService
{
    /// <summary>
    /// 获取教育记录列表
    /// </summary>
    /// <param name="memberId">成员ID筛选</param>
    /// <param name="subject">科目筛选</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>教育记录列表</returns>
    Task<List<EducationRecord>> GetRecordsAsync(long? memberId = null, string? subject = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加问答记录
    /// </summary>
    /// <param name="question">问题</param>
    /// <param name="answer">答案</param>
    /// <param name="subject">科目</param>
    /// <param name="memberId">成员ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新记录</returns>
    Task<EducationRecord> AddQaAsync(string question, string answer, string? subject = null, long? memberId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除记录
    /// </summary>
    /// <param name="id">记录ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
