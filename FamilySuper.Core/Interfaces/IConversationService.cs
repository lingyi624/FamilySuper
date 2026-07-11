using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 会话服务接口
/// </summary>
public interface IConversationService
{
    /// <summary>
    /// 获取会话列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>会话列表</returns>
    Task<List<ConversationSession>> GetSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建会话
    /// </summary>
    /// <param name="title">会话标题</param>
    /// <param name="memberId">成员ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新会话</returns>
    Task<ConversationSession> CreateSessionAsync(string? title = null, long? memberId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取会话消息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>消息列表</returns>
    Task<List<ConversationMessage>> GetMessagesAsync(long sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加消息
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="role">角色</param>
    /// <param name="content">内容</param>
    /// <param name="tokenCount">令牌数量</param>
    /// <param name="modelId">模型ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新消息</returns>
    Task<ConversationMessage> AddMessageAsync(long sessionId, MessageRole role, string content, int? tokenCount = null, string? modelId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除会话
    /// </summary>
    /// <param name="sessionId">会话ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteSessionAsync(long sessionId, CancellationToken cancellationToken = default);
}
