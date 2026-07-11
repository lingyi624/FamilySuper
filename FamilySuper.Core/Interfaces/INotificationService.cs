using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 通知服务接口
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// 通知事件
    /// </summary>
    event Action<NotificationItem>? OnNotification;

    /// <summary>
    /// 获取待处理通知
    /// </summary>
    /// <param name="limit">数量限制</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>通知列表</returns>
    Task<List<NotificationItem>> GetPendingAsync(int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// 推送通知
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="message">消息</param>
    /// <param name="category">分类</param>
    /// <param name="memberId">成员ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task PushAsync(string title, string message, string category, long? memberId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记为已读
    /// </summary>
    /// <param name="id">通知ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task MarkReadAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 标记所有为已读
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task MarkAllReadAsync(CancellationToken cancellationToken = default);
}
