using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 会议助手服务接口
/// </summary>
public interface IMeetingNoteService
{
    /// <summary>
    /// 获取会议记录列表
    /// </summary>
    /// <param name="status">状态筛选</param>
    /// <param name="from">起始日期</param>
    /// <param name="to">结束日期</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>会议记录列表</returns>
    Task<List<MeetingNote>> GetNotesAsync(string? status = null, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取会议记录
    /// </summary>
    /// <param name="id">记录ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>会议记录</returns>
    Task<MeetingNote?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加会议记录
    /// </summary>
    /// <param name="note">会议记录</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的会议记录</returns>
    Task<MeetingNote> AddAsync(MeetingNote note, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新会议记录
    /// </summary>
    /// <param name="note">会议记录</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task UpdateAsync(MeetingNote note, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除会议记录
    /// </summary>
    /// <param name="id">记录ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}