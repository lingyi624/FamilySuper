using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Interfaces;

public interface IConversationService
{
    Task<List<ConversationSession>> GetSessionsAsync(CancellationToken cancellationToken = default);
    Task<ConversationSession> CreateSessionAsync(string? title = null, long? memberId = null, CancellationToken cancellationToken = default);
    Task<List<ConversationMessage>> GetMessagesAsync(long sessionId, CancellationToken cancellationToken = default);
    Task<ConversationMessage> AddMessageAsync(long sessionId, MessageRole role, string content, int? tokenCount = null, string? modelId = null, CancellationToken cancellationToken = default);
    Task DeleteSessionAsync(long sessionId, CancellationToken cancellationToken = default);
}
