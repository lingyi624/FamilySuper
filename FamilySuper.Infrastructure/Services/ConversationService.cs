using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class ConversationService : IConversationService
{
    private readonly IRepository<ConversationSession> _sessionRepo;
    private readonly IRepository<ConversationMessage> _messageRepo;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(IRepository<ConversationSession> sessionRepo, IRepository<ConversationMessage> messageRepo, ILogger<ConversationService> logger)
    {
        _sessionRepo = sessionRepo;
        _messageRepo = messageRepo;
        _logger = logger;
    }

    public async Task<List<ConversationSession>> GetSessionsAsync(CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepo.GetAllAsync(cancellationToken);
        return sessions.OrderByDescending(s => s.LastMessageAt).ToList();
    }

    public async Task<ConversationSession> CreateSessionAsync(string? title = null, long? memberId = null, CancellationToken cancellationToken = default)
    {
        var session = new ConversationSession
        {
            Title = title ?? $"会话 {DateTime.Now:yyyy-MM-dd HH:mm}",
            MemberId = memberId,
            LastMessageAt = DateTime.UtcNow,
            MessageCount = 0
        };
        await _sessionRepo.AddAsync(session, cancellationToken);
        await _sessionRepo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("创建对话会话: {Title}", session.Title);
        return session;
    }

    public async Task<List<ConversationMessage>> GetMessagesAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        var messages = await _messageRepo.FindAsync(m => m.SessionId == sessionId, cancellationToken);
        return messages.OrderBy(m => m.CreatedAt).ToList();
    }

    public async Task<ConversationMessage> AddMessageAsync(long sessionId, MessageRole role, string content, int? tokenCount = null, string? modelId = null, CancellationToken cancellationToken = default)
    {
        var message = new ConversationMessage
        {
            SessionId = sessionId,
            Role = role,
            Content = content,
            TokenCount = tokenCount,
            ModelId = modelId,
            CreatedAt = DateTime.UtcNow
        };
        await _messageRepo.AddAsync(message, cancellationToken);

        var session = await _sessionRepo.GetByIdAsync(sessionId, cancellationToken);
        if (session is not null)
        {
            session.LastMessageAt = DateTime.UtcNow;
            session.MessageCount++;
            session.UpdatedAt = DateTime.UtcNow;
            if (role == MessageRole.User && string.IsNullOrEmpty(session.Title))
            {
                session.Title = content.Length > 30 ? content[..30] + "..." : content;
            }
        }

        await _messageRepo.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task DeleteSessionAsync(long sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, cancellationToken);
        if (session is null) return;
        session.IsDeleted = true;
        session.UpdatedAt = DateTime.UtcNow;

        var messages = await _messageRepo.FindAsync(m => m.SessionId == sessionId, cancellationToken);
        foreach (var msg in messages)
        {
            msg.IsDeleted = true;
            msg.UpdatedAt = DateTime.UtcNow;
        }

        await _messageRepo.SaveChangesAsync(cancellationToken);
    }
}
