using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Interfaces;

/// <summary>
/// 聊天消息记录
/// </summary>
/// <param name="Role">角色（user/assistant）</param>
/// <param name="Content">消息内容</param>
/// <param name="ImagePreview">图片预览地址</param>
public record ChatMessage(string Role, string Content, string? ImagePreview = null);

/// <summary>
/// AI智能服务接口
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// 文本聊天
    /// </summary>
    /// <param name="userMessage">用户消息</param>
    /// <param name="mode">应用模式</param>
    /// <param name="history">聊天历史</param>
    /// <param name="systemPromptOverride">系统提示词覆盖</param>
    /// <param name="reasoningEffort">推理强度</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>AI回复内容</returns>
    Task<string> ChatAsync(
        string userMessage,
        AppMode mode,
        List<ChatMessage> history,
        string? systemPromptOverride = null,
        string? reasoningEffort = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 图文聊天
    /// </summary>
    /// <param name="userMessage">用户消息</param>
    /// <param name="imageBytes">图片字节</param>
    /// <param name="imageMimeType">图片MIME类型</param>
    /// <param name="mode">应用模式</param>
    /// <param name="history">聊天历史</param>
    /// <param name="systemPromptOverride">系统提示词覆盖</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>AI回复内容</returns>
    Task<string> ChatWithImageAsync(
        string userMessage,
        byte[] imageBytes,
        string imageMimeType,
        AppMode mode,
        List<ChatMessage> history,
        string? systemPromptOverride = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// AI服务是否可用
    /// </summary>
    bool IsAvailable { get; }
}
